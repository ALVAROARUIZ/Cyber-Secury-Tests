"""Default entry point: DIVE VIEW in the browser."""

import uvicorn

from dive_view.config.settings import Settings
from dive_view.web.app import create_app


def main() -> int:
    """Serve the web application; collectors remain independent services."""
    settings = Settings()
    uvicorn.run(create_app(settings), host=settings.web_host, port=settings.web_port)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
