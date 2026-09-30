"""QThreadPool tasks for non-blocking database and analytics calls."""

from collections.abc import Callable

from PySide6.QtCore import QObject, QRunnable, Signal


class WorkerSignals(QObject):
    """Signals emitted from a background operation."""

    completed = Signal(object)
    failed = Signal(str)


class Worker(QRunnable):
    """Runs an argument-free callable outside the Qt GUI thread."""

    def __init__(self, operation: Callable[[], object]) -> None:
        super().__init__()
        self.operation = operation
        self.signals = WorkerSignals()

    def run(self) -> None:
        try:
            self.signals.completed.emit(self.operation())
        except Exception as error:
            self.signals.failed.emit(str(error))
