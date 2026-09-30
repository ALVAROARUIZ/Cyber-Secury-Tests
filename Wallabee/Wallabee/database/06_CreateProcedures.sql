USE [DiveViewLab];
GO

CREATE OR ALTER PROCEDURE reporting.usp_GetExperimentDashboard @ExperimentId UNIQUEIDENTIFIER AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM reporting.vw_ExperimentOverview WHERE ExperimentId = @ExperimentId;
    SELECT * FROM reporting.vw_RoundPerformance WHERE ExperimentId = @ExperimentId ORDER BY RoundNumber;
    SELECT s.DetectionLevel, COUNT_BIG(*) AS ScanCount, SUM(CASE WHEN s.WasDetected = 1 THEN 1 ELSE 0 END) AS DetectedCount
    FROM lab.Scans s WHERE s.ExperimentId = @ExperimentId GROUP BY s.DetectionLevel;
        SELECT DISTINCT CAST(AVG(s.DurationMs) OVER () AS DECIMAL(18,6)) AS AverageScanMs,
            CAST(PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY s.DurationMs) OVER () AS DECIMAL(18,6)) AS P50ScanMs,
           CAST(PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY s.DurationMs) OVER () AS DECIMAL(18,6)) AS P95ScanMs,
           CAST(PERCENTILE_CONT(0.99) WITHIN GROUP (ORDER BY s.DurationMs) OVER () AS DECIMAL(18,6)) AS P99ScanMs
        FROM lab.Scans s WHERE s.ExperimentId = @ExperimentId;
    SELECT CAST(AVG(m.CpuPercent) AS DECIMAL(7,3)) AS CpuAveragePercent, CAST(MAX(m.CpuPercent) AS DECIMAL(7,3)) AS CpuPeakPercent,
           CAST(AVG(m.MemoryUsedMb) AS DECIMAL(18,3)) AS MemoryAverageMb, CAST(MAX(m.MemoryUsedMb) AS DECIMAL(18,3)) AS MemoryPeakMb
    FROM telemetry.SystemMetricSamples m WHERE m.ExperimentId = @ExperimentId;
    SELECT d.IndicatorCode, d.IndicatorName, COUNT_BIG(*) AS TriggerCount, CAST(AVG(si.WeightApplied) AS DECIMAL(9,4)) AS AverageWeight
    FROM lab.ScanIndicators si JOIN lab.Scans s ON s.ScanId = si.ScanId JOIN lab.IndicatorDefinitions d ON d.IndicatorDefinitionId = si.IndicatorDefinitionId
    WHERE s.ExperimentId = @ExperimentId AND si.Triggered = 1 GROUP BY d.IndicatorCode, d.IndicatorName ORDER BY TriggerCount DESC;
END;
GO

CREATE OR ALTER PROCEDURE reporting.usp_GetVersionComparison
    @FromDate DATETIME2(3) = NULL, @ToDate DATETIME2(3) = NULL, @WallabeeVersionId INT = NULL, @HoneyBadgerVersionId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT w.WallabeeVersionId, w.Version AS WallabeeVersion, h.HoneyBadgerVersionId, h.Version AS HoneyBadgerVersion,
           COUNT(DISTINCT e.ExperimentId) AS ExperimentCount, COUNT_BIG(s.ScanId) AS ScanCount,
           CAST(AVG(r.DetectionRate) AS DECIMAL(9,6)) AS AverageDetectionRate, CAST(AVG(r.EvasionRate) AS DECIMAL(9,6)) AS AverageEvasionRate,
           CAST(AVG(r.PrecisionScore) AS DECIMAL(9,6)) AS AveragePrecision, CAST(AVG(r.RecallScore) AS DECIMAL(9,6)) AS AverageRecall,
           CAST(AVG(r.F1Score) AS DECIMAL(9,6)) AS AverageF1, CAST(AVG(r.AverageScanMs) AS DECIMAL(18,6)) AS AverageScanMs
    FROM lab.Experiments e JOIN lab.WallabeeVersions w ON w.WallabeeVersionId = e.WallabeeVersionId
    LEFT JOIN lab.HoneyBadgerVersions h ON h.HoneyBadgerVersionId = e.HoneyBadgerVersionId
    LEFT JOIN lab.ExperimentRounds r ON r.ExperimentId = e.ExperimentId LEFT JOIN lab.Scans s ON s.ExperimentId = e.ExperimentId
    WHERE (@FromDate IS NULL OR e.StartedAt >= @FromDate) AND (@ToDate IS NULL OR e.StartedAt < @ToDate)
      AND (@WallabeeVersionId IS NULL OR e.WallabeeVersionId = @WallabeeVersionId)
      AND (@HoneyBadgerVersionId IS NULL OR e.HoneyBadgerVersionId = @HoneyBadgerVersionId)
    GROUP BY w.WallabeeVersionId, w.Version, h.HoneyBadgerVersionId, h.Version;
END;
GO

CREATE OR ALTER PROCEDURE reporting.usp_GetRoundDetails @RoundId UNIQUEIDENTIFIER AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM reporting.vw_RoundPerformance WHERE RoundId = @RoundId;
    SELECT s.ScanId, s.SampleId, s.Sha256, s.FileName, s.FileSizeBytes, s.RiskScore, s.DetectionLevel, s.WasDetected, s.DurationMs, s.ErrorOccurred, s.ErrorCode
    FROM lab.Scans s WHERE s.RoundId = @RoundId ORDER BY s.StartedAt;
    SELECT si.ScanId, d.IndicatorCode, d.IndicatorName, si.WeightApplied, si.ObservedValue, si.Triggered
    FROM lab.ScanIndicators si JOIN lab.Scans s ON s.ScanId = si.ScanId JOIN lab.IndicatorDefinitions d ON d.IndicatorDefinitionId = si.IndicatorDefinitionId
    WHERE s.RoundId = @RoundId ORDER BY si.ScanId, d.IndicatorCode;
    SELECT * FROM telemetry.SystemMetricSamples WHERE RoundId = @RoundId ORDER BY TimestampUtc;
END;
GO

CREATE OR ALTER PROCEDURE reporting.usp_GetFalseNegatives
    @ExperimentId UNIQUEIDENTIFIER = NULL, @RoundId UNIQUEIDENTIFIER = NULL, @WallabeeVersionId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.ScanId, s.ExperimentId, s.RoundId, s.SampleId, sm.Sha256, sm.FileName, sm.SampleFamily, sm.SampleType, s.RiskScore, s.DetectionLevel, s.StartedAt
    FROM lab.Scans s JOIN lab.Samples sm ON sm.SampleId = s.SampleId
    WHERE sm.GroundTruthLabel = 'Malicious' AND s.WasDetected = 0 AND (@ExperimentId IS NULL OR s.ExperimentId = @ExperimentId)
      AND (@RoundId IS NULL OR s.RoundId = @RoundId) AND (@WallabeeVersionId IS NULL OR s.WallabeeVersionId = @WallabeeVersionId);
END;
GO

CREATE OR ALTER PROCEDURE reporting.usp_GetFalsePositives
    @ExperimentId UNIQUEIDENTIFIER = NULL, @RoundId UNIQUEIDENTIFIER = NULL, @WallabeeVersionId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.ScanId, s.ExperimentId, s.RoundId, s.SampleId, sm.Sha256, sm.FileName, sm.SampleFamily, sm.SampleType, s.RiskScore, s.DetectionLevel, s.StartedAt
    FROM lab.Scans s JOIN lab.Samples sm ON sm.SampleId = s.SampleId
    WHERE sm.GroundTruthLabel = 'Benign' AND s.WasDetected = 1 AND (@ExperimentId IS NULL OR s.ExperimentId = @ExperimentId)
      AND (@RoundId IS NULL OR s.RoundId = @RoundId) AND (@WallabeeVersionId IS NULL OR s.WallabeeVersionId = @WallabeeVersionId);
END;
GO

CREATE OR ALTER PROCEDURE reporting.usp_GetPerformanceTrend
    @FromDate DATETIME2(3) = NULL, @ToDate DATETIME2(3) = NULL, @WallabeeVersionId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.* FROM reporting.vw_LatencyTrend t JOIN lab.WallabeeVersions w ON w.Version = t.WallabeeVersion
    WHERE (@FromDate IS NULL OR t.[Date] >= CAST(@FromDate AS date)) AND (@ToDate IS NULL OR t.[Date] < CAST(@ToDate AS date))
      AND (@WallabeeVersionId IS NULL OR w.WallabeeVersionId = @WallabeeVersionId) ORDER BY t.[Date], t.WallabeeVersion;
END;
GO

CREATE OR ALTER PROCEDURE reporting.usp_GetTopIndicators @Top INT = 20 AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top) * FROM reporting.vw_IndicatorFrequency ORDER BY TriggerCount DESC, IndicatorCode;
END;
GO
