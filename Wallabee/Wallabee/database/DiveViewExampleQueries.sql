USE [DiveViewLab];
GO

-- 1. Latest experiments
SELECT TOP (25) * FROM reporting.vw_ExperimentOverview ORDER BY StartedAt DESC;
-- 2. Best detection rate per Wallabee version
SELECT WallabeeVersion, MAX(AverageDetectionRate) AS BestDetectionRate FROM reporting.vw_VersionComparison GROUP BY WallabeeVersion;
-- 3. P95 evolution
SELECT * FROM reporting.vw_LatencyTrend ORDER BY [Date], WallabeeVersion;
-- 4. Honey Badger evasion
SELECT * FROM reporting.vw_HoneyBadgerVersionPerformance ORDER BY EvasionRate DESC;
-- 5. Average CPU and 6. peak RAM per experiment
SELECT ExperimentName, CpuAveragePercent, MemoryPeakMb FROM reporting.vw_ExperimentOverview;
-- 7. Frequent indicators
EXEC reporting.usp_GetTopIndicators @Top = 20;
-- 8. False negatives and 9. false positives
EXEC reporting.usp_GetFalseNegatives;
EXEC reporting.usp_GetFalsePositives;
-- 10. Wallabee version comparison
SELECT * FROM reporting.vw_VersionComparison ORDER BY AverageDetectionRate DESC;
-- 11. Round evolution
SELECT * FROM reporting.vw_RoundPerformance ORDER BY ExperimentId, RoundNumber;
-- 12. Retained sample metadata
SELECT SampleId, ExperimentId, RoundId, Sha256, RetainReason, ExternalStorageReference FROM lab.Samples WHERE RetainSample = 1;
-- 13. Throughput by host
SELECT h.HostName, AVG(r.FilesPerSecond) AS AverageFilesPerSecond FROM lab.ExperimentRounds r JOIN lab.Experiments e ON e.ExperimentId = r.ExperimentId LEFT JOIN lab.TestHosts h ON h.TestHostId = e.TestHostId GROUP BY h.HostName;
-- 14. Benchmarks by date
SELECT CAST(StartedAt AS date) AS BenchmarkDate, COUNT(*) AS Runs, AVG(AverageScanMs) AS AverageScanMs FROM benchmark.BenchmarkRuns GROUP BY CAST(StartedAt AS date) ORDER BY BenchmarkDate;
-- 15. Risk score distribution
SELECT WallabeeVersionId, CASE WHEN RiskScore < 20 THEN '0-19' WHEN RiskScore < 50 THEN '20-49' WHEN RiskScore < 75 THEN '50-74' WHEN RiskScore < 90 THEN '75-89' ELSE '90-100' END AS RiskBand, COUNT_BIG(*) AS ScanCount FROM lab.Scans GROUP BY WallabeeVersionId, CASE WHEN RiskScore < 20 THEN '0-19' WHEN RiskScore < 50 THEN '20-49' WHEN RiskScore < 75 THEN '50-74' WHEN RiskScore < 90 THEN '75-89' ELSE '90-100' END;
GO