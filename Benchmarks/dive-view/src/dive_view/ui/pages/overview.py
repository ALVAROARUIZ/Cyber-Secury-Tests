"""Overview dashboard page."""

import pandas as pd
from PySide6.QtWidgets import QGridLayout, QLabel, QScrollArea, QVBoxLayout, QWidget

from dive_view.ui.charts import LineChart
from dive_view.ui.widgets import MetricCard


class OverviewPage(QScrollArea):
    """High-level metrics and trends for the selected scope."""

    def __init__(self, parent: QWidget | None = None) -> None:
        super().__init__(parent)
        self.setWidgetResizable(True)
        content = QWidget()
        layout = QVBoxLayout(content)
        layout.setContentsMargins(24, 24, 24, 24)
        layout.setSpacing(16)
        heading = QLabel("Overview")
        heading.setObjectName("pageTitle")
        layout.addWidget(heading)
        self._cards: dict[str, MetricCard] = {}
        grid = QGridLayout()
        for index, name in enumerate(
            [
                "Total Experiments",
                "Total Rounds",
                "Total Scans",
                "Detection Rate",
                "Evasion Rate",
                "Average F1",
                "Average Scan Time",
                "P95 Latency",
                "Files/sec",
                "Peak RAM",
                "Peak CPU",
            ]
        ):
            card = MetricCard(name)
            self._cards[name] = card
            grid.addWidget(card, index // 4, index % 4)
        layout.addLayout(grid)
        charts = QGridLayout()
        self._detection = LineChart("Detection and evasion rate")
        self._latency = LineChart("P95 latency and throughput")
        self._resources = LineChart("CPU and RAM")
        charts.addWidget(self._detection, 0, 0)
        charts.addWidget(self._latency, 0, 1)
        charts.addWidget(self._resources, 1, 0, 1, 2)
        layout.addLayout(charts)
        self.setWidget(content)

    def load(self, overview: dict[str, object]) -> None:
        metrics = overview.get("metrics", {})
        if isinstance(metrics, dict):
            for name, value in metrics.items():
                if name in self._cards:
                    self._cards[name].set_value(self._format_metric(name, value))
        trends = overview.get("trends")
        if isinstance(trends, pd.DataFrame) and not trends.empty:
            self._detection.set_series(trends, "Timestamp", ["Detection Rate", "Evasion Rate"])
            self._latency.set_series(trends, "Timestamp", ["P95 Latency", "Files/sec"])
            self._resources.set_series(trends, "Timestamp", ["CPU", "RAM"])

    @staticmethod
    def _format_metric(name: str, value: object) -> str:
        if isinstance(value, float):
            if "Rate" in name or name == "Average F1":
                return f"{value:.1%}"
            return f"{value:.1f}"
        return f"{value:,}" if isinstance(value, int) else str(value)
