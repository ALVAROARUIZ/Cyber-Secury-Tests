"""Generic report page for scoped data tables."""

import pandas as pd
from PySide6.QtWidgets import QLabel, QVBoxLayout, QWidget

from dive_view.ui.widgets import DataTable


class TablePage(QWidget):
    """Displays a named report dataset."""

    def __init__(self, title: str, description: str, parent: QWidget | None = None) -> None:
        super().__init__(parent)
        layout = QVBoxLayout(self)
        layout.setContentsMargins(24, 24, 24, 24)
        heading = QLabel(title)
        heading.setObjectName("pageTitle")
        detail = QLabel(description)
        detail.setObjectName("pageSubtitle")
        self.table = DataTable()
        layout.addWidget(heading)
        layout.addWidget(detail)
        layout.addWidget(self.table)

    def set_frame(self, frame: pd.DataFrame) -> None:
        self.table.set_frame(frame)
