"""Version-comparison calculations."""

from collections.abc import Mapping


def calculate_relative_change(baseline: float, candidate: float) -> float | None:
    """Return the percent change from baseline, or None when undefined."""
    if baseline == 0:
        return None
    return (candidate - baseline) / abs(baseline) * 100


def compare_versions(
    baseline: Mapping[str, float], candidate: Mapping[str, float]
) -> list[dict[str, float | str | None]]:
    """Compare shared named metrics without assigning a qualitative verdict."""
    rows: list[dict[str, float | str | None]] = []
    for metric in sorted(baseline.keys() & candidate.keys()):
        previous, current = baseline[metric], candidate[metric]
        delta = current - previous
        rows.append(
            {
                "metric": metric,
                "baseline": previous,
                "candidate": current,
                "absolute_delta": delta,
                "relative_delta": calculate_relative_change(previous, current),
                "direction": "increased"
                if delta > 0
                else "decreased"
                if delta < 0
                else "unchanged",
            }
        )
    return rows
