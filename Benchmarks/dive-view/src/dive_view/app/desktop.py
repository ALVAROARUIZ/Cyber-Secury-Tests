"""Executable DIVE VIEW entry point."""

import logging
import sys
from pathlib import Path

if __package__ in {None, ""}:
    sys.path.insert(0, str(Path(__file__).parents[2]))

from PySide6.QtWidgets import QApplication

from dive_view.app.bootstrap import build_service
from dive_view.config.settings import Settings
from dive_view.ui.main_window import MainWindow


def main() -> int:
    """Launch the desktop application."""
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    settings = Settings()
    app = QApplication(sys.argv)
    theme = Path(__file__).parents[1] / "ui" / "themes" / "dive_dark.qss"
    app.setStyleSheet(theme.read_text(encoding="utf-8"))
    service, status = build_service(settings)
    window = MainWindow(service, status)
    window.show()
    return app.exec()


if __name__ == "__main__":
    raise SystemExit(main())
