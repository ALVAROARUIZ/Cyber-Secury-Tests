"""Guarded statistical-significance tests."""

from typing import Any

import numpy as np
import pandas as pd
from scipy import stats


def compare_samples(
    baseline: pd.Series, candidate: pd.Series, test: str = "welch"
) -> dict[str, Any]:
    """Run a requested test only when both samples contain adequate numeric observations."""
    left = pd.to_numeric(baseline, errors="coerce").dropna().to_numpy()
    right = pd.to_numeric(candidate, errors="coerce").dropna().to_numpy()
    if len(left) < 3 or len(right) < 3:
        return {
            "performed": False,
            "reason": "Each sample requires at least three complete observations.",
        }
    if test == "welch":
        statistic, p_value = stats.ttest_ind(left, right, equal_var=False)
        test_name = "Welch t-test"
    elif test == "mann_whitney":
        statistic, p_value = stats.mannwhitneyu(left, right, alternative="two-sided")
        test_name = "Mann-Whitney U"
    else:
        raise ValueError("Unsupported significance test.")
    return {
        "performed": True,
        "test": test_name,
        "statistic": float(statistic),
        "p_value": float(p_value),
        "baseline_size": len(left),
        "candidate_size": len(right),
    }


def calculate_confidence_intervals(
    values: pd.Series, confidence: float = 0.95
) -> dict[str, float | int] | None:
    """Return a t-distribution confidence interval for the population mean."""
    clean = pd.to_numeric(values, errors="coerce").dropna().to_numpy()
    if len(clean) < 2 or not 0 < confidence < 1:
        return None
    mean = float(np.mean(clean))
    margin = float(stats.sem(clean) * stats.t.ppf((1 + confidence) / 2, len(clean) - 1))
    return {"sample_size": len(clean), "mean": mean, "lower": mean - margin, "upper": mean + margin}
