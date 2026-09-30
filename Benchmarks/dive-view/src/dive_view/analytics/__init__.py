"""DataFrame-based analytics independent from the UI."""

from .comparative import calculate_relative_change, compare_versions
from .descriptive import calculate_latency_statistics
from .detection import calculate_confusion_matrix, calculate_detection_metrics

__all__ = [
    "calculate_confusion_matrix",
    "calculate_detection_metrics",
    "calculate_latency_statistics",
    "calculate_relative_change",
    "compare_versions",
]
