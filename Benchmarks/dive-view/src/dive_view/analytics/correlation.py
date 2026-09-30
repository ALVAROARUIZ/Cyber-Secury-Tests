"""Correlation analysis with explicit non-causal interpretation."""

import pandas as pd


def calculate_correlations(
    frame: pd.DataFrame, left: str, right: str
) -> dict[str, float | int | str]:
    """Calculate Pearson and Spearman values after dropping incomplete pairs."""
    pairs = frame[[left, right]].apply(pd.to_numeric, errors="coerce").dropna()
    if len(pairs) < 3:
        return {
            "sample_size": len(pairs),
            "note": "At least three complete observations are required.",
        }
    return {
        "sample_size": len(pairs),
        "pearson": float(pairs[left].corr(pairs[right], method="pearson")),
        "spearman": float(pairs[left].corr(pairs[right], method="spearman")),
        "note": "Correlation does not imply causation.",
    }
