"""Paged scan access designed for large tables."""

from dataclasses import dataclass

import pandas as pd
from sqlalchemy import text
from sqlalchemy.exc import SQLAlchemyError

from dive_view.common.exceptions import DatabaseUnavailableError, InvalidFilterError
from dive_view.data.database import Database


@dataclass(frozen=True, slots=True)
class ScanPage:
    """A bounded page of scans and total matching rows."""

    rows: pd.DataFrame
    total_rows: int
    page: int
    page_size: int


class ScanRepository:
    """Queries precisely selected scan fields with OFFSET/FETCH pagination."""

    _columns = (
        "ScanId, Experiment, Round, Sample, SHA256, FileName, Size, RiskScore, "
        "DetectionLevel, Detected, DurationMs, WallabeeVersion, Timestamp"
    )

    def __init__(self, database: Database) -> None:
        self._database = database

    def fetch_page(
        self,
        page: int = 1,
        page_size: int = 50,
        experiment: str | None = None,
        detected: bool | None = None,
        hash_prefix: str | None = None,
    ) -> ScanPage:
        """Fetch one page, never loading the complete scans history into memory."""
        if page < 1 or page_size not in {50, 100, 250, 500}:
            raise InvalidFilterError(
                "Page must be positive and page size must be 50, 100, 250, or 500."
            )
        clauses: list[str] = []
        params: dict[str, str | int | bool] = {
            "offset": (page - 1) * page_size,
            "page_size": page_size,
        }
        if experiment:
            clauses.append("Experiment = :experiment")
            params["experiment"] = experiment
        if detected is not None:
            clauses.append("Detected = :detected")
            params["detected"] = detected
        if hash_prefix:
            clauses.append("SHA256 LIKE :hash_prefix")
            params["hash_prefix"] = f"{hash_prefix}%"
        where = f" WHERE {' AND '.join(clauses)}" if clauses else ""
        page_query = text(
            f"SELECT {self._columns} FROM reporting.vw_ScanExplorer{where} "
            "ORDER BY Timestamp DESC OFFSET :offset ROWS FETCH NEXT :page_size ROWS ONLY"
        )
        count_query = text(f"SELECT COUNT_BIG(1) FROM reporting.vw_ScanExplorer{where}")
        try:
            with self._database.engine.connect() as connection:
                rows = pd.read_sql(page_query, connection, params=params)
                total = int(connection.execute(count_query, params).scalar_one())
        except SQLAlchemyError as error:
            raise DatabaseUnavailableError("Scan explorer view is unavailable.") from error
        return ScanPage(rows=rows, total_rows=total, page=page, page_size=page_size)
