"""SQL Server engine lifecycle and lightweight health checks."""

from time import perf_counter

from sqlalchemy import Engine, create_engine, text
from sqlalchemy.exc import SQLAlchemyError

from dive_view.common.exceptions import DatabaseUnavailableError
from dive_view.config.settings import Settings


class Database:
    """Owns the SQLAlchemy engine used by repositories."""

    def __init__(self, settings: Settings) -> None:
        self._settings = settings
        self._engine: Engine | None = None

    @property
    def engine(self) -> Engine:
        if self._engine is None:
            self._engine = create_engine(
                self._settings.sqlalchemy_url,
                pool_pre_ping=True,
                pool_recycle=1800,
            )
        return self._engine

    def check_health(self) -> tuple[str, float]:
        """Return connected, slow, or disconnected and measured query latency in ms."""
        started = perf_counter()
        try:
            with self.engine.connect() as connection:
                connection.execute(text("SELECT 1"))
        except SQLAlchemyError as error:
            raise DatabaseUnavailableError("Unable to connect to DiveViewLab.") from error
        latency_ms = (perf_counter() - started) * 1000
        return ("slow" if latency_ms > 500 else "connected", latency_ms)

    def dispose(self) -> None:
        if self._engine is not None:
            self._engine.dispose()
