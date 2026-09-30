import pandas as pd

from dive_view.analytics.significance import calculate_confidence_intervals, compare_samples


def test_significance_requires_minimum_sample_size() -> None:
    result = compare_samples(pd.Series([1, 2]), pd.Series([2, 3]))
    assert result["performed"] is False


def test_confidence_interval_has_ordered_bounds() -> None:
    result = calculate_confidence_intervals(pd.Series([1, 2, 3, 4, 5]))
    assert result is not None
    assert result["lower"] < result["mean"] < result["upper"]
