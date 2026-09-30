"""Read-only access to reporting views."""

from collections.abc import Sequence

import pandas as pd
from sqlalchemy import text
from sqlalchemy.exc import SQLAlchemyError

from dive_view.common.exceptions import DatabaseUnavailableError
from dive_view.data.database import Database


class ReportingRepository:
    """Fetches approved reporting views, never mutating lab data."""

    OVERVIEW_VIEW = "reporting.vw_ExperimentOverview"
    ROUND_VIEW = "reporting.vw_RoundPerformance"
    VERSION_VIEW = "reporting.vw_VersionComparison"
    INDICATOR_VIEW = "reporting.vw_IndicatorFrequency"
    LATENCY_VIEW = "reporting.vw_LatencyTrend"

    def __init__(self, database: Database) -> None:
        self._database = database

    def fetch_view(self, view_name: str, columns: Sequence[str], limit: int = 500) -> pd.DataFrame:
        """Fetch selected columns from an approved reporting view with a hard limit."""
        if view_name not in {
            self.OVERVIEW_VIEW,
            self.ROUND_VIEW,
            self.VERSION_VIEW,
            self.INDICATOR_VIEW,
            self.LATENCY_VIEW,
        }:
            raise ValueError("Unsupported reporting view.")
        selected_columns = ", ".join(f"[{column}]" for column in columns)
        query = text(f"SELECT TOP (:limit) {selected_columns} FROM {view_name}")
        try:
            with self._database.engine.connect() as connection:
                return pd.read_sql(query, connection, params={"limit": limit})
        except SQLAlchemyError as error:
            raise DatabaseUnavailableError(
                f"Reporting view '{view_name}' is unavailable."
            ) from error
