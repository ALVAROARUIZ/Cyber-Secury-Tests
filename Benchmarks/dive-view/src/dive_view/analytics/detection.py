"""Detection quality metrics."""


import pandas as pd


def calculate_confusion_matrix(actual: pd.Series, predicted: pd.Series) -> dict[str, int]:
    """Calculate binary confusion-matrix counts from truth and detection flags."""
    pairs = pd.DataFrame({"actual": actual, "predicted": predicted}).dropna()
    truth = pairs["actual"].astype(bool)
    observed = pairs["predicted"].astype(bool)
    return {
        "true_positive": int((truth & observed).sum()),
        "true_negative": int((~truth & ~observed).sum()),
        "false_positive": int((~truth & observed).sum()),
        "false_negative": int((truth & ~observed).sum()),
    }


def calculate_detection_metrics(actual: pd.Series, predicted: pd.Series) -> dict[str, float | int]:
    """Calculate detection metrics with zero-safe denominators."""
    matrix = calculate_confusion_matrix(actual, predicted)
    tp, tn = matrix["true_positive"], matrix["true_negative"]
    fp, fn = matrix["false_positive"], matrix["false_negative"]

    def divide(numerator: float, denominator: float) -> float:
        return numerator / denominator if denominator else 0.0

    precision = divide(tp, tp + fp)
    recall = divide(tp, tp + fn)
    return {
        **matrix,
        "accuracy": divide(tp + tn, tp + tn + fp + fn),
        "precision": precision,
        "recall": recall,
        "specificity": divide(tn, tn + fp),
        "f1": divide(2 * precision * recall, precision + recall),
        "false_positive_rate": divide(fp, fp + tn),
        "false_negative_rate": divide(fn, fn + tp),
        "detection_rate": recall,
        "evasion_rate": divide(fn, fn + tp),
    }
