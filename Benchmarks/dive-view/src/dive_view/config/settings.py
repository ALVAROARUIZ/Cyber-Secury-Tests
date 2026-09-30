"""Environment-backed application settings."""

from pathlib import Path
from urllib.parse import quote_plus

from pydantic import Field, model_validator
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Runtime settings loaded from environment variables or a local .env file."""

    model_config = SettingsConfigDict(env_file=".env", env_prefix="DIVE_", extra="ignore")

    db_server: str = "localhost"
    db_name: str = "DiveViewLab"
    db_driver: str = "ODBC Driver 18 for SQL Server"
    db_trusted_connection: bool = True
    db_username: str | None = None
    db_password: str | None = Field(default=None, repr=False)
    db_timeout_seconds: int = 15
    view_demo_mode: bool = False
    cache_ttl_seconds: int = 30
    refresh_interval_seconds: int = Field(default=5, ge=1, le=300)
    web_host: str = "127.0.0.1"
    web_port: int = Field(default=8790, ge=1, le=65535)
    export_directory: Path = Path("exports")

    @model_validator(mode="after")
    def validate_authentication(self) -> "Settings":
        if not self.db_trusted_connection and not (self.db_username and self.db_password):
            raise ValueError(
                "DIVE_DB_USERNAME and DIVE_DB_PASSWORD are required without trusted auth."
            )
        return self

    @property
    def sqlalchemy_url(self) -> str:
        """Build a SQLAlchemy URL without exposing the password in configuration files."""
        attributes = [
            f"DRIVER={{{self.db_driver}}}",
            f"SERVER={self.db_server}",
            f"DATABASE={self.db_name}",
            "TrustServerCertificate=yes",
            f"Connection Timeout={self.db_timeout_seconds}",
        ]
        if self.db_trusted_connection:
            attributes.append("Trusted_Connection=yes")
        else:
            attributes.extend([f"UID={self.db_username}", f"PWD={self.db_password}"])
        return f"mssql+pyodbc:///?odbc_connect={quote_plus(';'.join(attributes))}"
