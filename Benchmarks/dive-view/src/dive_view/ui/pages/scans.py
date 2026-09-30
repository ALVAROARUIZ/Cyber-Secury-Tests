"""Paged scan explorer."""

from PySide6.QtCore import Signal
from PySide6.QtWidgets import (
    QComboBox,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QPushButton,
    QVBoxLayout,
    QWidget,
)

from dive_view.data.repositories.scans import ScanPage
from dive_view.ui.widgets import DataTable


class ScansPage(QWidget):
    """Virtualized scan viewer with constrained page sizes."""

    page_requested = Signal(int, int, str)

    def __init__(self, parent: QWidget | None = None) -> None:
        super().__init__(parent)
        self._page = 1
        self._total = 0
        layout = QVBoxLayout(self)
        layout.setContentsMargins(24, 24, 24, 24)
        title = QLabel("Scans")
        title.setObjectName("pageTitle")
        layout.addWidget(title)
        controls = QHBoxLayout()
        self._search = QLineEdit()
        self._search.setPlaceholderText("SHA256 prefix")
        self._size = QComboBox()
        self._size.addItems(["50", "100", "250", "500"])
        refresh = QPushButton("Search")
        refresh.clicked.connect(self._request_first_page)
        controls.addWidget(self._search)
        controls.addWidget(self._size)
        controls.addWidget(refresh)
        controls.addStretch()
        layout.addLayout(controls)
        self.table = DataTable()
        layout.addWidget(self.table)
        pagination = QHBoxLayout()
        self._previous = QPushButton("Previous")
        self._next = QPushButton("Next")
        self._status = QLabel()
        self._previous.clicked.connect(lambda: self._request_page(self._page - 1))
        self._next.clicked.connect(lambda: self._request_page(self._page + 1))
        pagination.addWidget(self._previous)
        pagination.addWidget(self._next)
        pagination.addWidget(self._status)
        pagination.addStretch()
        layout.addLayout(pagination)
        self._update_controls()

    def load(self, result: object) -> None:
        if not isinstance(result, ScanPage):
            return
        self._page, self._total = result.page, result.total_rows
        self.table.set_frame(result.rows)
        self._update_controls()

    def _request_first_page(self) -> None:
        self._request_page(1)

    def _request_page(self, page: int) -> None:
        if page < 1 or (page - 1) * int(self._size.currentText()) >= self._total:
            return
        self.page_requested.emit(page, int(self._size.currentText()), self._search.text().strip())

    def _update_controls(self) -> None:
        page_size = int(self._size.currentText())
        self._previous.setEnabled(self._page > 1)
        self._next.setEnabled(self._page * page_size < self._total)
        self._status.setText(f"Page {self._page} | {self._total:,} matching scans")
