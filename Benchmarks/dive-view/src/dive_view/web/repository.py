"""Bounded, parameterized read models for the shared laboratory database."""

import json
from typing import Any

import pandas as pd
from sqlalchemy import text
from sqlalchemy.exc import SQLAlchemyError

from dive_view.common.exceptions import DatabaseUnavailableError
from dive_view.data.database import Database


def records(frame: pd.DataFrame) -> list[dict[str, Any]]:
    """Normalize dates, Decimal, numpy scalars and missing values to JSON values."""
    frame = frame.copy()
    # SQL datetime2 values in DiveViewLab are UTC but have no timezone marker.
    for column in frame.select_dtypes(include=["datetime", "datetimetz"]).columns:
        frame[column] = pd.to_datetime(frame[column], utc=True)
    result: list[dict[str, Any]] = json.loads(frame.to_json(orient="records", date_format="iso"))
    return result


SCANS = """
SELECT CONVERT(varchar(36),s.ScanId) AS Id, s.FileName, s.ExternalExperimentId AS Experiment,
 s.ExternalRoundId AS Round, s.ExternalSampleId AS Sample, s.Sha256 AS SHA256,
 s.FileSizeBytes AS Size, s.RiskScore, s.DetectionLevel, s.WasDetected AS Detected,
 s.DurationMs, w.Version AS WallabeeVersion, s.CompletedAt AS Timestamp, s.ResponseAction
FROM lab.Scans s JOIN lab.WallabeeVersions w ON w.WallabeeVersionId=s.WallabeeVersionId
"""
EVENTS = """
SELECT CONCAT('telemetry:',EventId) AS Id, EventType, SourceComponent AS Source,
 Severity, State, TimestampUtc AS Timestamp, CONVERT(varchar(36),ExperimentId) AS Experiment,
 CONVERT(varchar(36),RoundId) AS Round, CONVERT(varchar(36),ScanId) AS ScanId,
 CONVERT(varchar(36),BenchmarkId) AS BenchmarkId, Message
FROM telemetry.TelemetryEvents
UNION ALL
SELECT CONCAT('experiment:',ExperimentEventId), EventType, 'Experiment', 'Info', 'Recorded',
 OccurredAt, CONVERT(varchar(36),ExperimentId), CONVERT(varchar(36),RoundId), NULL, NULL, Message
FROM lab.ExperimentEvents
UNION ALL
SELECT CONCAT('quarantine:',QuarantineEventId), Action, 'Quarantine', 'Info', State,
 OccurredAt, NULL, NULL, CONVERT(varchar(36),ScanId), NULL, Reason
FROM lab.QuarantineEvents
UNION ALL
SELECT CONCAT('retry:',Id), 'TelemetryRetryFailed', 'Telemetry', 'Warning', 'Failed',
 TimestampUtc, NULL, NULL, NULL, NULL, CONCAT('Event ',EventId,' / attempt ',Attempt)
FROM telemetry.TelemetryFailures
"""

# SQL and search columns are server-owned. No identifier comes from request input.
TABLES: dict[str, tuple[str, str, list[str]]] = {
    "scans": (
        SCANS,
        "Timestamp DESC, Id DESC",
        ["Id", "FileName", "SHA256", "Experiment", "Round"],
    ),
    "events": (EVENTS, "Timestamp DESC, Id DESC", ["EventType", "Source", "Experiment", "ScanId"]),
    "experiments": (
        """
        SELECT CONVERT(varchar(36),e.ExperimentId) AS Id, e.ExternalExperimentId AS Experiment,
        e.ExperimentName AS Name, e.Status, e.StartedAt AS Timestamp,
        w.Version AS WallabeeVersion, h.Version AS HoneyBadgerVersion,
        (SELECT COUNT_BIG(*) FROM lab.ExperimentRounds r WHERE r.ExperimentId=e.ExperimentId)
            AS Rounds,
        (SELECT COUNT_BIG(*) FROM lab.Scans s WHERE s.ExperimentId=e.ExperimentId) AS Scans
        FROM lab.Experiments e
        JOIN lab.WallabeeVersions w ON w.WallabeeVersionId=e.WallabeeVersionId
        LEFT JOIN lab.HoneyBadgerVersions h ON h.HoneyBadgerVersionId=e.HoneyBadgerVersionId
    """,
        "Timestamp DESC, Id DESC",
        ["Experiment", "Name", "WallabeeVersion"],
    ),
    "rounds": (
        """
        SELECT CONVERT(varchar(36),RoundId) AS Id, ExternalRoundId AS Round,
        CONVERT(varchar(36),ExperimentId) AS Experiment, StartedAt AS Timestamp,
        SamplesScanned, DetectionRate, EvasionRate, PrecisionScore, RecallScore, F1Score,
        P95ScanMs, P99ScanMs, FilesPerSecond FROM lab.ExperimentRounds
    """,
        "Timestamp DESC, Id DESC",
        ["Round", "Experiment"],
    ),
    "benchmarks": (
        """
        SELECT CONVERT(varchar(36),b.BenchmarkId) AS Id, b.ExternalExperimentId AS Experiment,
        b.ExternalRoundId AS Round, b.CompletedAt AS Timestamp, w.Version AS WallabeeVersion,
        b.SamplesScanned, b.LabeledSamples, b.TruePositives, b.TrueNegatives,
        b.FalsePositives, b.FalseNegatives, b.DetectionRate, b.EvasionRate,
        b.PrecisionScore, b.RecallScore, b.F1Score, b.P95ScanMs, b.P99ScanMs,
        b.FilesPerSecond, b.MbPerSecond, b.CpuAveragePercent, b.MemoryPeakMb
        FROM benchmark.BenchmarkRuns b
        JOIN lab.WallabeeVersions w ON w.WallabeeVersionId=b.WallabeeVersionId
    """,
        "Timestamp DESC, Id DESC",
        ["Experiment", "Round", "WallabeeVersion"],
    ),
    "systems": (
        """
        SELECT CONVERT(varchar(36),TestHostId) AS Id, HostName, HostAlias, OperatingSystem,
        Architecture, CpuLogicalCores, TotalMemoryMb, EnvironmentName, IsActive,
        LastSeenAt AS Timestamp FROM lab.TestHosts
    """,
        "Timestamp DESC, Id DESC",
        ["HostName", "HostAlias", "EnvironmentName"],
    ),
    "resources": (
        """
        SELECT CONVERT(varchar(36),SystemMetricSampleId) AS Id,
        CONVERT(varchar(36),BenchmarkId) AS BenchmarkId,
        CONVERT(varchar(36),TestHostId) AS HostId, TimestampUtc AS Timestamp,
        CpuPercent, MemoryUsedMb, ProcessCpuPercent, ProcessMemoryMb, ThreadCount
        FROM telemetry.SystemMetricSamples
    """,
        "Timestamp DESC, Id DESC",
        ["HostId", "BenchmarkId"],
    ),
    "versions": (
        """
        SELECT CONCAT('wallabee:',WallabeeVersionId) AS Id, 'Wallabee' AS Component,
        Version, CommitHash, BuildDate AS Timestamp FROM lab.WallabeeVersions
        UNION ALL
        SELECT CONCAT('honeybadger:',HoneyBadgerVersionId), 'Honey Badger',
        Version, CommitHash, BuildDate FROM lab.HoneyBadgerVersions
    """,
        "Component, Version, Id",
        ["Component", "Version", "CommitHash"],
    ),
    "indicators": (
        """
        SELECT IndicatorCode AS Id, IndicatorName, TriggerCount, DistinctScanCount,
        AverageWeight, PercentageOfScans FROM reporting.vw_IndicatorFrequency
    """,
        "TriggerCount DESC, Id",
        ["Id", "IndicatorName"],
    ),
}


class WebRepository:
    """Reads actual persisted events, without enabling optional HTTP export or writes."""

    def __init__(self, database: Database) -> None:
        self.database = database

    def query(self, sql: str, params: dict[str, Any] | None = None) -> list[dict[str, Any]]:
        try:
            with self.database.engine.connect() as connection:
                return records(pd.read_sql(text(sql), connection, params=params or {}))
        except SQLAlchemyError as error:
            raise DatabaseUnavailableError("No se pudo consultar DiveViewLab.") from error

    def page(
        self, kind: str, page: int, size: int, search: str = "", level: str = ""
    ) -> dict[str, Any]:
        sql, order, columns = TABLES[kind]
        filters: list[str] = []
        params: dict[str, Any] = {"offset": (page - 1) * size, "size": size}
        if search:
            filters.append(
                "(" + " OR ".join(f"[{column}] LIKE :search ESCAPE '!'" for column in columns) + ")"
            )
            escaped = search.replace("!", "!!").replace("%", "!%")
            params["search"] = "%" + escaped.replace("_", "!_").replace("[", "![") + "%"
        if level and kind == "scans":
            filters.append("DetectionLevel = :level")
            params["level"] = level
        where = " WHERE " + " AND ".join(filters) if filters else ""
        total = self.query(f"SELECT COUNT_BIG(*) AS Total FROM ({sql}) q{where}", params)[0]
        rows = self.query(
            f"SELECT * FROM ({sql}) q{where} ORDER BY {order} "
            "OFFSET :offset ROWS FETCH NEXT :size ROWS ONLY",
            params,
        )
        return {"rows": rows, "total": total["Total"], "page": page, "size": size}

    def scan_detail(self, scan_id: str) -> dict[str, Any] | None:
        rows = self.query(f"SELECT * FROM ({SCANS}) q WHERE Id=:id", {"id": scan_id})
        if not rows:
            return None
        rows[0]["Indicators"] = self.query(
            "SELECT Name, Source, Description, WeightApplied AS Weight, Triggered "
            "FROM lab.ScanIndicators WHERE ScanId=CONVERT(uniqueidentifier,:id) "
            "ORDER BY CreatedAt, ScanIndicatorId",
            {"id": scan_id},
        )
        return rows[0]

    def overview(self) -> dict[str, Any]:
        metrics = self.query("""
            SELECT (SELECT COUNT_BIG(*) FROM lab.Experiments) AS Experiments,
            (SELECT COUNT_BIG(*) FROM lab.ExperimentRounds) AS Rounds,
            (SELECT COUNT_BIG(*) FROM lab.Scans) AS Scans,
            (SELECT COUNT_BIG(*) FROM lab.Scans WHERE WasDetected=1) AS Detected,
            (SELECT AVG(DurationMs) FROM lab.Scans) AS AverageMs,
            (SELECT COUNT_BIG(*) FROM telemetry.TelemetryEvents) AS TelemetryEvents,
            (SELECT COUNT_BIG(*) FROM lab.ExperimentEvents) AS ExperimentEvents,
            (SELECT COUNT_BIG(*) FROM lab.QuarantineEvents) AS QuarantineEvents,
            (SELECT COUNT_BIG(*) FROM telemetry.TelemetryFailures) AS RetryFailures
        """)[0]
        metrics["Events"] = sum(
            metrics[k]
            for k in ("TelemetryEvents", "ExperimentEvents", "QuarantineEvents", "RetryFailures")
        )
        trends = self.query("""
            SELECT TOP (30) CAST(CompletedAt AS date) AS Day, COUNT_BIG(*) AS Scans,
            SUM(CASE WHEN WasDetected=1 THEN 1 ELSE 0 END) AS Detected,
            AVG(DurationMs) AS AverageMs FROM lab.Scans
            GROUP BY CAST(CompletedAt AS date) ORDER BY Day DESC
        """)
        risk = self.query("""
            SELECT DetectionLevel AS Level, COUNT_BIG(*) AS Count FROM lab.Scans
            GROUP BY DetectionLevel ORDER BY DetectionLevel
        """)
        benchmark = self.page("benchmarks", 1, 1)["rows"]
        return {
            "metrics": metrics,
            "trends": list(reversed(trends)),
            "risk": risk,
            "benchmark": benchmark[0] if benchmark else None,
            "events": self.page("events", 1, 8)["rows"],
        }
