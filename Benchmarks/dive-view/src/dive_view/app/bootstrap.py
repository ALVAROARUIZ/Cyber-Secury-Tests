"""Dependency composition root."""

import logging

from dive_view.config.settings import Settings
from dive_view.data.database import Database
from dive_view.data.repositories.scans import ScanRepository
from dive_view.services.dashboard_service import DashboardService


def build_service(settings: Settings) -> tuple[DashboardService, str]:
    """Build service graph and allow the app to start when SQL Server is unavailable."""
    if settings.view_demo_mode:
        logging.info("ApplicationStarted demo_mode=true")
        return DashboardService(True, settings.cache_ttl_seconds), "Demo mode"
    database = Database(settings)
    try:
        state, latency = database.check_health()
        logging.info("DatabaseConnected latency_ms=%.1f", latency)
        return DashboardService(False, settings.cache_ttl_seconds, ScanRepository(database)), state
    except Exception:
        logging.exception("DatabaseDisconnected")
        return DashboardService(False, settings.cache_ttl_seconds), "Disconnected"
