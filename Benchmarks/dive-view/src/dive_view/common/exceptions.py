"""Application-specific exceptions."""


class DiveViewError(Exception):
    """Base exception for expected application errors."""


class DatabaseUnavailableError(DiveViewError):
    """The SQL Server connection could not be established."""


class QueryTimeoutError(DiveViewError):
    """A database query exceeded its permitted duration."""


class InvalidFilterError(DiveViewError):
    """A requested query filter is invalid."""
