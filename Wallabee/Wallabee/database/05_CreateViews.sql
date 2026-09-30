USE [DiveViewLab];
GO

CREATE OR ALTER VIEW reporting.vw_ExperimentOverview AS
SELECT e.ExperimentId, e.ExperimentName, e.Status, e.StartedAt, e.CompletedAt,
       w.Version AS WallabeeVersion, h.Version AS HoneyBadgerVersion,
       e.TotalRounds, e.TotalSamples, ISNULL(scanStats.TotalScans, 0) AS TotalScans,
       ISNULL(scanStats.SamplesDetected, 0) AS SamplesDetected, ISNULL(scanStats.SamplesMissed, 0) AS SamplesMissed,
       CAST(scanStats.SamplesDetected * 1.0 / NULLIF(scanStats.TotalScans, 0) AS DECIMAL(9,6)) AS DetectionRate,
       CAST(scanStats.SamplesMissed * 1.0 / NULLIF(scanStats.TotalScans, 0) AS DECIMAL(9,6)) AS EvasionRate,
       scanStats.AverageRiskScore, scanStats.AverageScanMs, roundStats.P95ScanMs, roundStats.P99ScanMs,
       roundStats.FilesPerSecond, roundStats.CpuAveragePercent, roundStats.CpuPeakPercent,
       roundStats.MemoryAverageMb, roundStats.MemoryPeakMb
FROM lab.Experiments e
JOIN lab.WallabeeVersions w ON w.WallabeeVersionId = e.WallabeeVersionId
LEFT JOIN lab.HoneyBadgerVersions h ON h.HoneyBadgerVersionId = e.HoneyBadgerVersionId
OUTER APPLY (
    SELECT COUNT_BIG(*) AS TotalScans, SUM(CASE WHEN s.WasDetected = 1 THEN 1 ELSE 0 END) AS SamplesDetected,
           SUM(CASE WHEN s.WasDetected = 0 THEN 1 ELSE 0 END) AS SamplesMissed,
           CAST(AVG(s.RiskScore) AS DECIMAL(9,4)) AS AverageRiskScore,
           CAST(AVG(s.DurationMs) AS DECIMAL(18,6)) AS AverageScanMs
    FROM lab.Scans s WHERE s.ExperimentId = e.ExperimentId
) scanStats
OUTER APPLY (
    SELECT CAST(AVG(r.P95ScanMs) AS DECIMAL(18,6)) AS P95ScanMs, CAST(AVG(r.P99ScanMs) AS DECIMAL(18,6)) AS P99ScanMs,
           CAST(AVG(r.FilesPerSecond) AS DECIMAL(18,4)) AS FilesPerSecond, CAST(AVG(r.CpuAveragePercent) AS DECIMAL(7,3)) AS CpuAveragePercent,
           CAST(MAX(r.CpuPeakPercent) AS DECIMAL(7,3)) AS CpuPeakPercent, CAST(AVG(r.MemoryAverageMb) AS DECIMAL(18,3)) AS MemoryAverageMb,
           CAST(MAX(r.MemoryPeakMb) AS DECIMAL(18,3)) AS MemoryPeakMb
    FROM lab.ExperimentRounds r WHERE r.ExperimentId = e.ExperimentId
) roundStats;
GO

CREATE OR ALTER VIEW reporting.vw_RoundPerformance AS
SELECT r.ExperimentId, r.RoundId, r.RoundNumber, r.GenerationNumber, r.SamplesScanned, r.SamplesDetected,
       r.SamplesMissed, r.DetectionRate, r.EvasionRate, r.PrecisionScore AS Precision, r.RecallScore AS Recall,
       r.F1Score AS F1, r.AverageScanMs, r.P95ScanMs AS P95, r.P99ScanMs AS P99, r.FilesPerSecond,
       r.MbPerSecond, r.CpuAveragePercent AS CPU, r.MemoryAverageMb AS Memory
FROM lab.ExperimentRounds r;
GO

CREATE OR ALTER VIEW reporting.vw_VersionComparison AS
SELECT w.Version AS WallabeeVersion, COUNT(DISTINCT e.ExperimentId) AS ExperimentCount,
       COUNT_BIG(s.ScanId) AS ScanCount, CAST(AVG(r.DetectionRate) AS DECIMAL(9,6)) AS AverageDetectionRate,
       CAST(AVG(r.EvasionRate) AS DECIMAL(9,6)) AS AverageEvasionRate, CAST(AVG(r.PrecisionScore) AS DECIMAL(9,6)) AS AveragePrecision,
       CAST(AVG(r.RecallScore) AS DECIMAL(9,6)) AS AverageRecall, CAST(AVG(r.F1Score) AS DECIMAL(9,6)) AS AverageF1,
       CAST(AVG(r.AverageScanMs) AS DECIMAL(18,6)) AS AverageScanMs, CAST(AVG(r.P95ScanMs) AS DECIMAL(18,6)) AS AverageP95,
       CAST(AVG(r.P99ScanMs) AS DECIMAL(18,6)) AS AverageP99, CAST(AVG(r.FilesPerSecond) AS DECIMAL(18,4)) AS AverageFilesPerSecond,
       CAST(AVG(r.CpuAveragePercent) AS DECIMAL(7,3)) AS AverageCpu, CAST(AVG(r.MemoryAverageMb) AS DECIMAL(18,3)) AS AverageMemory
FROM lab.WallabeeVersions w
LEFT JOIN lab.Experiments e ON e.WallabeeVersionId = w.WallabeeVersionId
LEFT JOIN lab.ExperimentRounds r ON r.ExperimentId = e.ExperimentId
LEFT JOIN lab.Scans s ON s.ExperimentId = e.ExperimentId
GROUP BY w.Version;
GO

CREATE OR ALTER VIEW reporting.vw_HoneyBadgerVersionPerformance AS
SELECT h.Version AS HoneyBadgerVersion, COUNT(DISTINCT e.ExperimentId) AS Experiments, COUNT_BIG(s.SampleId) AS Samples,
       CAST(AVG(r.EvasionRate) AS DECIMAL(9,6)) AS EvasionRate, CAST(AVG(r.DetectionRate) AS DECIMAL(9,6)) AS DetectionRate,
       CAST(AVG(s.FitnessScore) AS DECIMAL(9,6)) AS AverageFitness,
       CAST(AVG(CASE WHEN s.MutationStrategy IS NOT NULL AND s.FitnessScore IS NOT NULL THEN CAST(1 AS DECIMAL(9,6)) ELSE CAST(0 AS DECIMAL(9,6)) END) AS DECIMAL(9,6)) AS AverageMutationSuccess
FROM lab.HoneyBadgerVersions h
LEFT JOIN lab.Experiments e ON e.HoneyBadgerVersionId = h.HoneyBadgerVersionId
LEFT JOIN lab.ExperimentRounds r ON r.ExperimentId = e.ExperimentId
LEFT JOIN lab.Samples s ON s.ExperimentId = e.ExperimentId
GROUP BY h.Version;
GO

CREATE OR ALTER VIEW reporting.vw_IndicatorFrequency AS
SELECT d.IndicatorCode, d.IndicatorName, COUNT_BIG(si.ScanIndicatorId) AS TriggerCount,
       COUNT(DISTINCT si.ScanId) AS DistinctScanCount, CAST(AVG(si.WeightApplied) AS DECIMAL(9,4)) AS AverageWeight,
       CAST(COUNT(DISTINCT si.ScanId) * 1.0 / NULLIF(total.TotalScans, 0) AS DECIMAL(9,6)) AS PercentageOfScans
FROM lab.IndicatorDefinitions d
LEFT JOIN lab.ScanIndicators si ON si.IndicatorDefinitionId = d.IndicatorDefinitionId AND si.Triggered = 1
CROSS JOIN (SELECT COUNT_BIG(*) AS TotalScans FROM lab.Scans) total
GROUP BY d.IndicatorCode, d.IndicatorName, total.TotalScans;
GO

CREATE OR ALTER VIEW reporting.vw_DetectionLevelDistribution AS
SELECT w.Version AS WallabeeVersion, s.DetectionLevel, COUNT_BIG(*) AS [Count],
       CAST(COUNT_BIG(*) * 1.0 / NULLIF(SUM(COUNT_BIG(*)) OVER (PARTITION BY w.WallabeeVersionId), 0) AS DECIMAL(9,6)) AS Percentage
FROM lab.Scans s
JOIN lab.WallabeeVersions w ON w.WallabeeVersionId = s.WallabeeVersionId
GROUP BY w.WallabeeVersionId, w.Version, s.DetectionLevel;
GO

CREATE OR ALTER VIEW reporting.vw_LatencyTrend AS
WITH latency AS (
    SELECT CAST(s.StartedAt AS date) AS [Date], s.WallabeeVersionId, s.DurationMs,
           PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY s.DurationMs) OVER (PARTITION BY CAST(s.StartedAt AS date), s.WallabeeVersionId) AS P50,
           PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY s.DurationMs) OVER (PARTITION BY CAST(s.StartedAt AS date), s.WallabeeVersionId) AS P95,
           PERCENTILE_CONT(0.99) WITHIN GROUP (ORDER BY s.DurationMs) OVER (PARTITION BY CAST(s.StartedAt AS date), s.WallabeeVersionId) AS P99
    FROM lab.Scans s
)
SELECT l.[Date], w.Version AS WallabeeVersion, CAST(AVG(l.DurationMs) AS DECIMAL(18,6)) AS AverageScanMs,
       CAST(MAX(l.P50) AS DECIMAL(18,6)) AS P50, CAST(MAX(l.P95) AS DECIMAL(18,6)) AS P95, CAST(MAX(l.P99) AS DECIMAL(18,6)) AS P99
FROM latency l
JOIN lab.WallabeeVersions w ON w.WallabeeVersionId = l.WallabeeVersionId
GROUP BY l.[Date], w.Version;
GO

CREATE OR ALTER VIEW reporting.vw_ExperimentTimeline AS
SELECT e.ExperimentId, CAST(NULL AS UNIQUEIDENTIFIER) AS RoundId, e.StartedAt AS OccurredAt,
       CAST('ExperimentStarted' AS VARCHAR(100)) AS EventType, CAST(e.ExperimentName AS NVARCHAR(1000)) AS Message
FROM lab.Experiments e
UNION ALL
SELECT r.ExperimentId, r.RoundId, r.StartedAt, 'RoundStarted', CONCAT(N'Round ', r.RoundNumber, N' started')
FROM lab.ExperimentRounds r
UNION ALL
SELECT ee.ExperimentId, ee.RoundId, ee.OccurredAt, ee.EventType, ee.Message
FROM lab.ExperimentEvents ee;
GO