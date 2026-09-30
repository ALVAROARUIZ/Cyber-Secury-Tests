"""Deterministic synthetic data for the offline demo mode."""

from datetime import UTC, datetime, timedelta

import numpy as np
import pandas as pd


def create_demo_dashboard() -> dict[str, object]:
    """Generate a coherent in-memory dashboard dataset without persisted samples."""
    random = np.random.default_rng(42)
    points = 30
    timestamps = pd.date_range(
        datetime.now(UTC) - timedelta(days=points - 1), periods=points, freq="D"
    )
    detection = np.clip(random.normal(0.93, 0.025, points), 0.82, 0.99)
    latency = np.clip(random.normal(38, 7, points), 15, 80)
    throughput = np.clip(random.normal(172, 22, points), 80, 250)
    return {
        "metrics": {
            "Total Experiments": 18,
            "Total Rounds": 104,
            "Total Scans": 248_607,
            "Detection Rate": float(detection.mean()),
            "Evasion Rate": float(1 - detection.mean()),
            "Average F1": 0.918,
            "Average Scan Time": float(latency.mean()),
            "P95 Latency": float(np.quantile(latency, 0.95)),
            "Files/sec": float(throughput.mean()),
            "Peak RAM": 812.0,
            "Peak CPU": 78.4,
        },
        "trends": pd.DataFrame(
            {
                "Timestamp": timestamps,
                "Detection Rate": detection,
                "Evasion Rate": 1 - detection,
                "P95 Latency": latency,
                "Files/sec": throughput,
                "CPU": np.clip(random.normal(58, 12, points), 10, 95),
                "RAM": np.clip(random.normal(620, 65, points), 300, 950),
            }
        ),
        "experiments": pd.DataFrame(
            {
                "Experiment Name": [f"HBW-{number:03d}" for number in range(1, 19)],
                "Status": ["Completed"] * 16 + ["Running", "Queued"],
                "Wallabee Version": ["0.4.0"] * 10 + ["0.3.0"] * 8,
                "Honey Badger Version": ["1.2.0"] * 18,
                "Rounds": random.integers(3, 12, 18),
                "Samples": random.integers(800, 6000, 18),
                "Detection Rate": np.clip(random.normal(0.93, 0.03, 18), 0.75, 0.99),
                "F1": np.clip(random.normal(0.91, 0.04, 18), 0.70, 0.98),
            }
        ),
    }


def create_demo_scans(total: int = 1_275) -> pd.DataFrame:
    """Generate a local virtualized scan dataset with no files written to disk."""
    random = np.random.default_rng(7)
    timestamps = pd.date_range(datetime.now(UTC) - timedelta(hours=total), periods=total, freq="h")
    detected = random.choice([True, False], size=total, p=[0.92, 0.08])
    return pd.DataFrame(
        {
            "ScanId": np.arange(1, total + 1),
            "Experiment": [f"HBW-{value:03d}" for value in random.integers(1, 19, total)],
            "Round": random.integers(1, 12, total),
            "Sample": [f"sample-{value:05d}" for value in range(total)],
            "SHA256": [f"{value:064x}" for value in random.integers(0, 2**31, total)],
            "FileName": [f"artifact-{value:05d}.bin" for value in range(total)],
            "Size": random.integers(1_000, 5_000_000, total),
            "RiskScore": random.integers(0, 101, total),
            "DetectionLevel": np.where(detected, "High", "None"),
            "Detected": detected,
            "DurationMs": np.clip(random.normal(38, 7, total), 10, 100).round(2),
            "WallabeeVersion": random.choice(["0.3.0", "0.4.0"], total),
            "Timestamp": timestamps,
        }
    )
