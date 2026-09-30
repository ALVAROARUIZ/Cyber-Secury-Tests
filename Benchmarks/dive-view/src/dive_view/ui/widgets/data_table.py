"""DataFrame-backed Qt table widget."""

import pandas as pd
from PySide6.QtCore import QAbstractTableModel, QModelIndex, QPersistentModelIndex, Qt
from PySide6.QtWidgets import QTableView, QWidget


class DataFrameModel(QAbstractTableModel):
    """Minimal pandas adapter for read-only report tables."""

    def __init__(self, frame: pd.DataFrame | None = None) -> None:
        super().__init__()
        self._frame = frame if frame is not None else pd.DataFrame()

    def update(self, frame: pd.DataFrame) -> None:
        self.beginResetModel()
        self._frame = frame
        self.endResetModel()

    def rowCount(self, parent: QModelIndex | QPersistentModelIndex | None = None) -> int:  # noqa: N802
        return 0 if parent is not None and parent.isValid() else len(self._frame.index)

    def columnCount(self, parent: QModelIndex | QPersistentModelIndex | None = None) -> int:  # noqa: N802
        return 0 if parent is not None and parent.isValid() else len(self._frame.columns)

    def data(  # noqa: N802
        self, index: QModelIndex | QPersistentModelIndex, role: int = Qt.ItemDataRole.DisplayRole
    ) -> object:
        if not index.isValid() or role != Qt.ItemDataRole.DisplayRole:
            return None
        value = self._frame.iloc[index.row(), index.column()]
        if isinstance(value, float):
            return f"{value:.3f}"
        return str(value)

    def headerData(  # noqa: N802
        self, section: int, orientation: Qt.Orientation, role: int = Qt.ItemDataRole.DisplayRole
    ) -> object:
        if role != Qt.ItemDataRole.DisplayRole:
            return None
        if orientation == Qt.Orientation.Horizontal:
            return str(self._frame.columns[section])
        return str(section + 1)


class DataTable(QTableView):
    """Consistently styled, sortable report table."""

    def __init__(self, parent: QWidget | None = None) -> None:
        super().__init__(parent)
        self.frame_model = DataFrameModel()
        self.setModel(self.frame_model)
        self.setAlternatingRowColors(True)
        self.setSortingEnabled(True)
        self.setSelectionBehavior(QTableView.SelectionBehavior.SelectRows)
        self.setSelectionMode(QTableView.SelectionMode.SingleSelection)
        self.horizontalHeader().setStretchLastSection(True)
        self.verticalHeader().setVisible(False)

    def set_frame(self, frame: pd.DataFrame) -> None:
        self.frame_model.update(frame)
        self.resizeColumnsToContents()
