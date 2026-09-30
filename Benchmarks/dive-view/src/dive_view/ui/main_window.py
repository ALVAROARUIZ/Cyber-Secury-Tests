"""DIVE VIEW main window and navigation coordinator."""

from collections.abc import Callable
from datetime import datetime

import pandas as pd
from PySide6.QtCore import QThreadPool, QTimer
from PySide6.QtWidgets import (
    QButtonGroup,
    QComboBox,
    QFrame,
    QHBoxLayout,
    QLabel,
    QMainWindow,
    QPushButton,
    QStackedWidget,
    QVBoxLayout,
    QWidget,
)

from dive_view.services.dashboard_service import DashboardService
from dive_view.ui.pages.overview import OverviewPage
from dive_view.ui.pages.scans import ScansPage
from dive_view.ui.pages.table_page import TablePage
from dive_view.ui.workers import Worker


class MainWindow(QMainWindow):
    """Composes navigation and delegates all data work to the service layer."""

    def __init__(self, service: DashboardService, status: str = "Demo mode") -> None:
        super().__init__()
        self._service = service
        self._pool = QThreadPool.globalInstance()
        self._timer = QTimer(self)
        self._timer.timeout.connect(self.refresh)
        self.setWindowTitle("DIVE VIEW | Honey Badger vs Wallabee")
        self.resize(1440, 900)
        root = QWidget()
        outer = QHBoxLayout(root)
        outer.setContentsMargins(0, 0, 0, 0)
        outer.setSpacing(0)
        outer.addWidget(self._create_sidebar())
        content = QVBoxLayout()
        content.setContentsMargins(0, 0, 0, 0)
        content.setSpacing(0)
        content.addWidget(self._create_topbar(status))
        self._pages = QStackedWidget()
        self._overview = OverviewPage()
        self._experiments = TablePage(
            "Experiments", "Searchable experiment outcomes and version metadata"
        )
        self._scans = ScansPage()
        self._scans.page_requested.connect(self._load_scans)
        self._page_map: dict[str, QWidget] = {
            "Overview": self._overview,
            "Experiments": self._experiments,
            "Rounds": TablePage("Rounds", "Round-level detection and performance metrics"),
            "Benchmarks": TablePage("Benchmarks", "Explicit numeric version comparisons"),
            "Scans": self._scans,
            "Detection": TablePage(
                "Detection", "Confusion matrix and detection quality indicators"
            ),
            "Performance": TablePage("Performance", "Latency and throughput analysis"),
            "System Metrics": TablePage("System Metrics", "Host and process resource telemetry"),
            "Versions": TablePage("Versions", "Wallabee and Honey Badger release outcomes"),
            "Indicators": TablePage("Indicators", "Indicator frequency and source analysis"),
            "Events": TablePage("Events", "Telemetry and application event timeline"),
            "Settings": TablePage("Settings", "Connection and application preferences"),
        }
        for page in self._page_map.values():
            self._pages.addWidget(page)
        content.addWidget(self._pages, 1)
        outer.addLayout(content, 1)
        self.setCentralWidget(root)
        self.refresh()
        self._load_scans(1, 50, "")

    def _create_sidebar(self) -> QFrame:
        sidebar = QFrame()
        sidebar.setObjectName("sidebar")
        sidebar.setFixedWidth(210)
        layout = QVBoxLayout(sidebar)
        layout.setContentsMargins(12, 18, 12, 18)
        brand = QLabel("DIVE VIEW")
        brand.setObjectName("pageTitle")
        layout.addWidget(brand)
        subtitle = QLabel("HONEY BADGER vs WALLABEE")
        subtitle.setObjectName("pageSubtitle")
        layout.addWidget(subtitle)
        layout.addSpacing(18)
        group = QButtonGroup(self)
        group.setExclusive(True)
        for index, name in enumerate(self._page_map_names()):
            button = QPushButton(name)
            button.setCheckable(True)
            button.clicked.connect(lambda _checked, page=name: self._show_page(page))
            group.addButton(button)
            layout.addWidget(button)
            if index == 0:
                button.setChecked(True)
        layout.addStretch()
        return sidebar

    def _create_topbar(self, status: str) -> QFrame:
        topbar = QFrame()
        topbar.setObjectName("topbar")
        layout = QHBoxLayout(topbar)
        layout.setContentsMargins(24, 10, 24, 10)
        self._connection = QLabel(status.upper())
        self._connection.setStyleSheet("color: #57D9A3; font-weight: 600;")
        self._updated = QLabel("Not refreshed")
        self._auto_refresh = QComboBox()
        self._auto_refresh.addItems(
            [
                "Auto refresh: Off",
                "Auto refresh: 5 sec",
                "Auto refresh: 15 sec",
                "Auto refresh: 30 sec",
                "Auto refresh: 1 min",
            ]
        )
        self._auto_refresh.currentIndexChanged.connect(self._set_auto_refresh)
        refresh = QPushButton("Refresh")
        refresh.clicked.connect(self.refresh)
        layout.addWidget(self._connection)
        layout.addStretch()
        layout.addWidget(QLabel("Experiment: All"))
        layout.addWidget(QLabel("Wallabee: All"))
        layout.addWidget(QLabel("Honey Badger: All"))
        layout.addWidget(self._auto_refresh)
        layout.addWidget(refresh)
        layout.addWidget(self._updated)
        return topbar

    @staticmethod
    def _page_map_names() -> list[str]:
        return [
            "Overview",
            "Experiments",
            "Rounds",
            "Benchmarks",
            "Scans",
            "Detection",
            "Performance",
            "System Metrics",
            "Versions",
            "Indicators",
            "Events",
            "Settings",
        ]

    def _show_page(self, name: str) -> None:
        self._pages.setCurrentWidget(self._page_map[name])

    def refresh(self) -> None:
        self._run(self._service.overview, self._load_overview)
        self._run(self._service.experiments, self._load_experiments)

    def _load_scans(self, page: int, page_size: int, search: str) -> None:
        self._run(lambda: self._service.scans_page(page, page_size, search), self._scans.load)

    def _load_overview(self, data: object) -> None:
        if isinstance(data, dict):
            self._overview.load(data)
        self._updated.setText(f"Updated {datetime.now().strftime('%H:%M:%S')}")

    def _load_experiments(self, data: object) -> None:
        self._experiments.set_frame(data if isinstance(data, pd.DataFrame) else pd.DataFrame())

    def _run(self, operation: Callable[[], object], on_success: Callable[[object], None]) -> None:
        worker = Worker(operation)
        worker.signals.completed.connect(on_success)
        worker.signals.failed.connect(self._show_error)
        self._pool.start(worker)

    def _show_error(self, message: str) -> None:
        self._connection.setText("DISCONNECTED")
        self._connection.setStyleSheet("color: #E16B75; font-weight: 600;")
        self._updated.setText(message)

    def _set_auto_refresh(self, index: int) -> None:
        intervals = [0, 5_000, 15_000, 30_000, 60_000]
        if intervals[index]:
            self._timer.start(intervals[index])
        else:
            self._timer.stop()
