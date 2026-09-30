"""Read service shared by web snapshots and paged explorers."""

from typing import Any

import numpy as np
import pandas as pd

from dive_view.services.demo_data import create_demo_scans
from dive_view.web.repository import WebRepository, records


class WebService:
    def __init__(self, repository: WebRepository | None) -> None:
        self.repository = repository
        self.demo: dict[str, pd.DataFrame] = {}
        if repository is None:
            scans = create_demo_scans().rename(columns={"ScanId": "Id"})
            scans["Id"] = scans["Id"].astype(str)
            scans["DetectionLevel"] = pd.cut(
                scans["RiskScore"],
                [-1, 19, 49, 74, 89, 100],
                labels=["Clean", "Low", "Medium", "High", "Critical"],
            ).astype(str)
            scans["Detected"] = scans["RiskScore"] >= 50
            self.demo["scans"] = scans
            self.demo["events"] = pd.DataFrame(
                {
                    "Id": "demo:" + scans["Id"],
                    "EventType": "ScanCompleted",
                    "Source": "Wallabee (demo)",
                    "Severity": "Info",
                    "State": "Recorded",
                    "Timestamp": scans["Timestamp"],
                    "Experiment": scans["Experiment"],
                    "Round": scans["Round"],
                    "ScanId": scans["Id"],
                    "Message": scans["FileName"],
                }
            )
            experiments = (
                scans.groupby("Experiment")
                .agg(
                    Scans=("Id", "count"),
                    Rounds=("Round", "nunique"),
                    Timestamp=("Timestamp", "max"),
                )
                .reset_index()
            )
            experiments["Id"] = experiments["Experiment"]
            self.demo["experiments"] = experiments

    def page(
        self, kind: str, page: int, size: int, search: str = "", level: str = ""
    ) -> dict[str, Any]:
        if self.repository:
            return self.repository.page(kind, page, size, search, level)
        frame = self.demo.get(kind, pd.DataFrame()).copy()
        if not frame.empty:
            if search:
                mask = (
                    frame.astype(str)
                    .apply(lambda column: column.str.contains(search, case=False, regex=False))
                    .any(axis=1)
                )
                frame = frame[mask]
            if level and kind == "scans":
                frame = frame[frame["DetectionLevel"] == level]
            if "Timestamp" in frame:
                frame = frame.sort_values(["Timestamp", "Id"], ascending=False)
        return {
            "rows": records(frame.iloc[(page - 1) * size : page * size]),
            "total": len(frame),
            "page": page,
            "size": size,
        }

    def scan_detail(self, scan_id: str) -> dict[str, Any] | None:
        if self.repository:
            return self.repository.scan_detail(scan_id)
        rows = records(self.demo["scans"][self.demo["scans"]["Id"] == scan_id])
        return rows[0] if rows else None

    def overview(self) -> dict[str, Any]:
        if self.repository:
            return self.repository.overview()
        scans = self.demo["scans"]
        daily = (
            scans.assign(Day=scans["Timestamp"].dt.floor("D"))
            .groupby("Day")
            .agg(
                Scans=("Id", "count"),
                Detected=("Detected", "sum"),
                AverageMs=("DurationMs", "mean"),
            )
            .reset_index()
            .tail(30)
        )
        risk = scans.groupby("DetectionLevel").size().reset_index(name="Count")
        return {
            "metrics": {
                "Experiments": len(self.demo["experiments"]),
                "Rounds": len(scans[["Experiment", "Round"]].drop_duplicates()),
                "Scans": len(scans),
                "Detected": int(np.sum(scans["Detected"])),
                "AverageMs": float(scans["DurationMs"].mean()),
                "Events": len(scans),
            },
            "trends": records(daily),
            "risk": records(risk.rename(columns={"DetectionLevel": "Level"})),
            "events": self.page("events", 1, 8)["rows"],
            "benchmark": None,
        }

    def close(self) -> None:
        if self.repository:
            self.repository.database.dispose()
