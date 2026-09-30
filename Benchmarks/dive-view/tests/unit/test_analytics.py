import pandas as pd
import pytest

from dive_view.analytics import (
    calculate_detection_metrics,
    calculate_latency_statistics,
    compare_versions,
)


def test_detection_metrics_calculates_confusion_matrix() -> None:
    metrics = calculate_detection_metrics(
        pd.Series([True, True, False, False]), pd.Series([True, False, True, False])
    )
    assert metrics["true_positive"] == 1
    assert metrics["true_negative"] == 1
    assert metrics["false_positive"] == 1
    assert metrics["false_negative"] == 1
    assert metrics["f1"] == 0.5


def test_latency_statistics_calculates_percentiles() -> None:
    stats = calculate_latency_statistics(pd.Series([1.0, 2.0, 3.0, 4.0, 5.0]))
    assert stats["count"] == 5
    assert stats["p50"] == 3.0
    assert stats["p95"] == 4.8


def test_compare_versions_reports_numeric_deltas() -> None:
    result = compare_versions({"p95": 4.3}, {"p95": 3.7})
    comparison = result[0]
    assert comparison["metric"] == "p95"
    assert comparison["direction"] == "decreased"
    assert comparison["absolute_delta"] == pytest.approx(-0.6)
    assert comparison["relative_delta"] == pytest.approx(-13.95348837209303)
