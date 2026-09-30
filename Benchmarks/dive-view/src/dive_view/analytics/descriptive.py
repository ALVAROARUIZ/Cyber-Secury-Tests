"""Descriptive statistical calculations."""

from typing import Any

import pandas as pd


def calculate_latency_statistics(values: pd.Series) -> dict[str, float | int]:
    """Return standard latency statistics, ignoring missing observations."""
    clean = pd.to_numeric(values, errors="coerce").dropna()
    if clean.empty:
        return {"count": 0}
    return {
        "count": int(clean.count()),
        "mean": float(clean.mean()),
        "median": float(clean.median()),
        "std": float(clean.std(ddof=1)) if len(clean) > 1 else 0.0,
        "variance": float(clean.var(ddof=1)) if len(clean) > 1 else 0.0,
        "min": float(clean.min()),
        "max": float(clean.max()),
        "q1": float(clean.quantile(0.25)),
        "q3": float(clean.quantile(0.75)),
        "iqr": float(clean.quantile(0.75) - clean.quantile(0.25)),
        "p50": float(clean.quantile(0.50)),
        "p90": float(clean.quantile(0.90)),
        "p95": float(clean.quantile(0.95)),
        "p99": float(clean.quantile(0.99)),
    }


def calculate_distribution(values: pd.Series, bins: int = 10) -> dict[str, Any]:
    """Calculate an equal-width histogram suitable for chart widgets."""
    clean = pd.to_numeric(values, errors="coerce").dropna()
    counts, edges = pd.cut(clean, bins=bins, include_lowest=True, retbins=True)
    return {"counts": counts.value_counts(sort=False).tolist(), "edges": edges.tolist()}
