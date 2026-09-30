import pandas as pd
import pytest

from dive_view.config.settings import Settings
from dive_view.services.dashboard_service import DashboardService


def test_settings_builds_trusted_sql_server_url() -> None:
    settings = Settings(db_server="lab-server", db_name="DiveViewLab")
    assert "SERVER%3Dlab-server" in settings.sqlalchemy_url
    assert "Trusted_Connection%3Dyes" in settings.sqlalchemy_url


def test_settings_requires_credentials_when_trusted_auth_is_disabled() -> None:
    with pytest.raises(ValueError, match="USERNAME"):
        Settings(db_trusted_connection=False)


def test_demo_scan_pagination_is_bounded() -> None:
    service = DashboardService(demo_mode=True, cache_ttl_seconds=30)
    result = service.scans_page(page=2, page_size=50)
    assert len(result.rows) == 50
    assert result.total_rows > 50
    assert result.page == 2
    assert isinstance(result.rows, pd.DataFrame)
