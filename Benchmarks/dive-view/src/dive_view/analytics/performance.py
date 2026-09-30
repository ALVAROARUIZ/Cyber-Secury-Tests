"""Throughput and resource statistics."""

import pandas as pd

from dive_view.analytics.descriptive import calculate_latency_statistics


def calculate_throughput_statistics(values: pd.Series) -> dict[str, float | int]:
    """Return standard descriptive summaries for files/sec or MB/sec series."""
    return calculate_latency_statistics(values)


def calculate_resource_statistics(
    cpu: pd.Series, memory: pd.Series
) -> dict[str, dict[str, float | int]]:
    """Calculate independently named CPU and memory resource summaries."""
    return {
        "cpu": calculate_latency_statistics(cpu),
        "memory": calculate_latency_statistics(memory),
    }
