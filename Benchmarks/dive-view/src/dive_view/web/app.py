"""FastAPI host: static UI, bounded queries and shared live snapshots."""

import asyncio
import json
import logging
from collections.abc import AsyncIterator
from contextlib import asynccontextmanager
from datetime import UTC, datetime
from pathlib import Path
from typing import Annotated, Any
from uuid import UUID

from fastapi import FastAPI, HTTPException, Query, Request
from fastapi.responses import FileResponse, JSONResponse, StreamingResponse
from fastapi.staticfiles import StaticFiles

from dive_view.common.exceptions import DatabaseUnavailableError
from dive_view.config.settings import Settings
from dive_view.data.database import Database
from dive_view.web.repository import TABLES, WebRepository
from dive_view.web.service import WebService

STATIC = Path(__file__).parent / "static"


class LiveState:
    def __init__(self, service: WebService, settings: Settings) -> None:
        self.service = service
        self.settings = settings
        self.snapshot: dict[str, Any] = {
            "available": False,
            "demo": settings.view_demo_mode,
            "updatedAt": None,
            "error": None,
            "data": None,
            "revision": 0,
        }

    async def refresh(self) -> None:
        try:
            data = await asyncio.to_thread(self.service.overview)
            self.snapshot = {
                "available": True,
                "demo": self.settings.view_demo_mode,
                "updatedAt": datetime.now(UTC).isoformat(),
                "error": None,
                "data": data,
                "revision": self.snapshot["revision"] + 1,
            }
        except Exception as error:
            logging.warning("DIVE VIEW refresh failed: %s", type(error).__name__)
            self.snapshot = {
                **self.snapshot,
                "available": False,
                "error": "No se pudo actualizar SQL Server. Reintentando conexión.",
                "revision": self.snapshot["revision"] + 1,
            }

    async def run(self, stop: asyncio.Event) -> None:
        while not stop.is_set():
            try:
                await asyncio.wait_for(stop.wait(), self.settings.refresh_interval_seconds)
            except TimeoutError:
                await self.refresh()


def create_app(settings: Settings | None = None, service: WebService | None = None) -> FastAPI:
    config = settings or Settings()
    if service is None:
        repository = None if config.view_demo_mode else WebRepository(Database(config))
        service = WebService(repository)
    live = LiveState(service, config)

    @asynccontextmanager
    async def lifespan(app: FastAPI) -> AsyncIterator[None]:
        stop = asyncio.Event()
        await live.refresh()
        task = asyncio.create_task(live.run(stop))
        try:
            yield
        finally:
            stop.set()
            await task
            await asyncio.to_thread(live.service.close)

    app = FastAPI(
        title="DIVE VIEW", version="0.2.0", lifespan=lifespan, docs_url=None, redoc_url=None
    )
    app.state.live = live
    app.mount("/static", StaticFiles(directory=STATIC), name="static")

    @app.exception_handler(DatabaseUnavailableError)
    async def database_error(request: Request, error: DatabaseUnavailableError) -> JSONResponse:
        return JSONResponse(
            {"detail": "SQL Server no está disponible para esta consulta."}, status_code=503
        )

    @app.get("/", include_in_schema=False)
    def index() -> FileResponse:
        return FileResponse(STATIC / "index.html")

    @app.get("/api/overview")
    def overview() -> dict[str, Any]:
        return live.snapshot

    @app.get("/api/config")
    def public_config() -> dict[str, Any]:
        return {
            "demo": config.view_demo_mode,
            "refreshSeconds": config.refresh_interval_seconds,
            "mode": "read-only",
            "application": "DIVE VIEW",
            "version": "0.2.0",
            "sources": [
                "TelemetryEvents",
                "ExperimentEvents",
                "QuarantineEvents",
                "TelemetryFailures",
            ],
            "database": config.db_name,
        }

    @app.get("/api/tables/{kind}")
    def table(
        kind: str,
        page: Annotated[int, Query(ge=1, le=1_000_000)] = 1,
        size: Annotated[int, Query(ge=1, le=100)] = 50,
        search: Annotated[str, Query(max_length=200)] = "",
        level: Annotated[str, Query(pattern="^(Clean|Low|Medium|High|Critical)?$")] = "",
    ) -> dict[str, Any]:
        if kind not in TABLES:
            raise HTTPException(404, "Vista no encontrada")
        return live.service.page(kind, page, size, search, level)

    @app.get("/api/scans/{scan_id}")
    def scan_detail(scan_id: str) -> dict[str, Any]:
        if not config.view_demo_mode:
            try:
                UUID(scan_id)
            except ValueError as error:
                raise HTTPException(422, "Identificador inválido") from error
        row = live.service.scan_detail(scan_id)
        if row is None:
            raise HTTPException(404, "Análisis no encontrado")
        return row

    @app.get("/api/live")
    async def events(request: Request) -> StreamingResponse:
        async def stream() -> AsyncIterator[str]:
            revision = -1
            while not await request.is_disconnected():
                snapshot = live.snapshot
                if revision != snapshot["revision"]:
                    revision = snapshot["revision"]
                    yield "event: snapshot\ndata: " + json.dumps(snapshot) + "\n\n"
                else:
                    yield ": heartbeat\n\n"
                await asyncio.sleep(1)

        return StreamingResponse(
            stream(),
            media_type="text/event-stream",
            headers={
                "Cache-Control": "no-cache, no-store",
                "X-Accel-Buffering": "no",
            },
        )

    return app
