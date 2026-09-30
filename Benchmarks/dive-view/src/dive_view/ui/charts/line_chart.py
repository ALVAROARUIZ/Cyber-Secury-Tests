"""Dark-theme Matplotlib line chart embedded in Qt."""

import pandas as pd
from matplotlib.backends.backend_qtagg import FigureCanvasQTAgg
from matplotlib.figure import Figure


class LineChart(FigureCanvasQTAgg):
    """Reusable line chart with a compact dark visual treatment."""

    def __init__(self, title: str, parent: object | None = None) -> None:
        self._figure = Figure(facecolor="#111820", layout="constrained")
        super().__init__(self._figure)  # type: ignore[no-untyped-call]
        self.setParent(parent)
        self._title = title

    def set_series(self, frame: pd.DataFrame, x_column: str, columns: list[str]) -> None:
        self._figure.clear()
        axis = self._figure.add_subplot()
        axis.set_facecolor("#111820")
        colors = ["#33B5E5", "#57D9A3", "#F3B562", "#E16B75"]
        for column, color in zip(columns, colors, strict=False):
            axis.plot(frame[x_column], frame[column], label=column, color=color, linewidth=1.8)
        axis.set_title(self._title, color="#E6EDF3", fontsize=10, loc="left", pad=12)
        axis.tick_params(colors="#8B949E", labelsize=8)
        for spine in axis.spines.values():
            spine.set_color("#263442")
        axis.grid(color="#263442", alpha=0.55, linewidth=0.6)
        axis.legend(frameon=False, labelcolor="#C9D1D9", fontsize=8, loc="best")
        self.draw_idle()  # type: ignore[no-untyped-call]
