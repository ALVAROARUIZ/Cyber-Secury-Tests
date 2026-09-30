# Analytics

The analytics engine receives pandas series and data frames, normalizes missing numeric values, and returns simple serializable dictionaries. It provides descriptive statistics through P99, detection metrics, deltas, correlation, and optional inference.

Outlier display should be based on IQR at the calling/reporting layer; no analytics function removes observations automatically.