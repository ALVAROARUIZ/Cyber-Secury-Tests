"""Dashboard-facing service that selects demo or repository data."""

import pandas as pd

from dive_view.data.repositories.scans import ScanPage, ScanRepository
from dive_view.services.cache import TtlCache
from dive_view.services.demo_data import create_demo_dashboard, create_demo_scans


class DashboardService:
    """Provides UI-safe read models without leaking repository details to widgets."""

    def __init__(
        self, demo_mode: bool, cache_ttl_seconds: int, scans: ScanRepository | None = None
    ) -> None:
        self._demo_mode = demo_mode
        self._cache = TtlCache(cache_ttl_seconds)
        self._scans = scans
        self._demo_data = create_demo_dashboard() if demo_mode else None
        self._demo_scans = create_demo_scans() if demo_mode else None

    def overview(self) -> dict[str, object]:
        if not self._demo_mode:
            return {"metrics": {}, "trends": pd.DataFrame(), "experiments": pd.DataFrame()}
        return self._cache.get_or_load("overview", lambda: self._demo_data or {})

    def experiments(self) -> pd.DataFrame:
        overview = self.overview()
        return (
            overview["experiments"]
            if isinstance(overview["experiments"], pd.DataFrame)
            else pd.DataFrame()
        )

    def scans_page(self, page: int, page_size: int, search: str = "") -> ScanPage:
        if self._demo_mode and self._demo_scans is not None:
            matching = self._demo_scans
            if search:
                matching = matching[matching["SHA256"].str.startswith(search.lower())]
            start = (page - 1) * page_size
            return ScanPage(
                matching.iloc[start : start + page_size].copy(), len(matching), page, page_size
            )
        if self._scans is None:
            return ScanPage(pd.DataFrame(), 0, page, page_size)
        return self._scans.fetch_page(page=page, page_size=page_size, hash_prefix=search or None)
