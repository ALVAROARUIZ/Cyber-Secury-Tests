"""Dashboard metric card."""

from PySide6.QtCore import Qt
from PySide6.QtWidgets import QFrame, QLabel, QVBoxLayout, QWidget


class MetricCard(QFrame):
    """Compact read-only metric display."""

    def __init__(self, label: str, value: str = "--", parent: QWidget | None = None) -> None:
        super().__init__(parent)
        self.setObjectName("metricCard")
        layout = QVBoxLayout(self)
        layout.setContentsMargins(14, 12, 14, 12)
        self._label = QLabel(label.upper())
        self._label.setObjectName("metricLabel")
        self._value = QLabel(value)
        self._value.setObjectName("metricValue")
        self._value.setAlignment(Qt.AlignmentFlag.AlignLeft)
        layout.addWidget(self._label)
        layout.addWidget(self._value)

    def set_value(self, value: str) -> None:
        self._value.setText(value)
