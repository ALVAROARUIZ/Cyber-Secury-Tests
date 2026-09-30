"""Small in-memory TTL cache for read-only dashboard data."""

from collections.abc import Callable
from time import monotonic
from typing import TypeVar

Value = TypeVar("Value")


class TtlCache:
    """Caches immutable read results for a configurable, brief period."""

    def __init__(self, ttl_seconds: int) -> None:
        self._ttl_seconds = ttl_seconds
        self._values: dict[str, tuple[float, object]] = {}

    def get_or_load(self, key: str, loader: Callable[[], Value]) -> Value:
        cached = self._values.get(key)
        if cached and monotonic() - cached[0] < self._ttl_seconds:
            return cached[1]  # type: ignore[return-value]
        value = loader()
        self._values[key] = (monotonic(), value)
        return value

    def clear(self) -> None:
        self._values.clear()
