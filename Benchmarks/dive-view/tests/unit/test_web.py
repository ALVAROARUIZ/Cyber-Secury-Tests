import asyncio
from typing import Any

import pandas as pd
from fastapi.testclient import TestClient

from dive_view.common.exceptions import DatabaseUnavailableError
from dive_view.config.settings import Settings
from dive_view.web.app import LiveState, create_app
from dive_view.web.repository import TABLES, WebRepository, records
from dive_view.web.service import WebService


def test_sql_timestamps_keep_utc_and_missing_values_remain_null() -> None:
    frame = pd.DataFrame(
        {"Timestamp": [pd.Timestamp("2026-09-29 18:00:00")], "Rate": [float("nan")]}
    )
    row = records(frame)[0]
    assert row["Timestamp"] == "2026-09-29T18:00:00.000Z"
    assert row["Rate"] is None


def test_web_serves_assets_and_consistent_demo_data() -> None:
    config = Settings(view_demo_mode=True)
    with TestClient(create_app(config)) as client:
        assert "DIVE VIEW" in client.get("/").text
        for asset in ["app.js", "app.css", "design-tokens.css"]:
            assert client.get("/static/" + asset).status_code == 200
        overview = client.get("/api/overview").json()
        assert overview["available"] and overview["demo"]
        scans = client.get("/api/tables/scans", params={"size": 25}).json()
        assert scans["total"] == overview["data"]["metrics"]["Scans"] == 1275
        assert len(scans["rows"]) == 25
        next_page = client.get("/api/tables/scans", params={"page": 2, "size": 25}).json()
        assert not {r["Id"] for r in scans["rows"]} & {r["Id"] for r in next_page["rows"]}
        row = scans["rows"][0]
        assert client.get("/api/scans/" + row["Id"]).json()["SHA256"] == row["SHA256"]
        filtered = client.get("/api/tables/scans", params={"level": "Critical"}).json()
        assert filtered["rows"]
        assert all(r["DetectionLevel"] == "Critical" for r in filtered["rows"])
        assert client.get("/api/tables/scans?search=unknown-nonexistent").json()["total"] == 0
        for kind in TABLES:
            assert client.get("/api/tables/" + kind).status_code == 200
        assert "password" not in client.get("/api/config").text.lower()


def test_web_rejects_unbounded_or_unknown_queries() -> None:
    with TestClient(create_app(Settings(view_demo_mode=True))) as client:
        for query in ["page=0", "page=1000001", "size=101", "level=invalid", "search=" + "a" * 201]:
            assert client.get("/api/tables/scans?" + query).status_code == 422
        assert client.get("/api/tables/private_table").status_code == 404
        assert client.get("/api/scans/missing").status_code == 404
        assert client.post("/api/tables/scans", json={}).status_code == 405


class FailingService(WebService):
    fail = False

    def overview(self) -> dict[str, Any]:
        if self.fail:
            raise DatabaseUnavailableError("password=must-not-leak")
        return super().overview()

    def page(
        self, kind: str, page: int, size: int, search: str = "", level: str = ""
    ) -> dict[str, Any]:
        if self.fail:
            raise DatabaseUnavailableError("password=must-not-leak")
        return super().page(kind, page, size, search, level)


def test_snapshot_retains_last_data_and_recovers_after_database_outage() -> None:
    service = FailingService(None)
    state = LiveState(service, Settings(view_demo_mode=True))

    async def exercise() -> None:
        await state.refresh()
        saved = state.snapshot["data"]
        updated = state.snapshot["updatedAt"]
        service.fail = True
        await state.refresh()
        assert state.snapshot["data"] == saved
        assert state.snapshot["updatedAt"] == updated
        assert not state.snapshot["available"]
        assert "must-not-leak" not in str(state.snapshot)
        service.fail = False
        await state.refresh()
        assert state.snapshot["available"] and state.snapshot["error"] is None

    asyncio.run(exercise())


def test_database_error_response_does_not_expose_internal_details() -> None:
    service = FailingService(None)
    service.fail = True
    with TestClient(create_app(Settings(view_demo_mode=True), service)) as client:
        response = client.get("/api/tables/events")
        assert response.status_code == 503
        assert "must-not-leak" not in response.text


def test_repository_search_is_bound_and_escapes_like_wildcards() -> None:
    calls: list[tuple[str, dict[str, Any]]] = []

    class CapturingRepository(WebRepository):
        def __init__(self) -> None:
            pass

        def query(self, sql: str, params: dict[str, Any] | None = None) -> list[dict[str, Any]]:
            calls.append((sql, params or {}))
            return [{"Total": 0}] if "COUNT_BIG(*) AS Total" in sql else []

    search = "' OR 1=1;--%_[!"
    CapturingRepository().page("scans", 3, 25, search)
    assert all(search not in sql for sql, _ in calls)
    assert calls[-1][1]["offset"] == 50
    assert calls[-1][1]["size"] == 25
    assert calls[-1][1]["search"] == "%' OR 1=1;--!%!_![!!%"
