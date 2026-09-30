USE [DiveViewLab];
GO

DECLARE @Missing TABLE (ObjectName NVARCHAR(256) NOT NULL);
INSERT @Missing (ObjectName)
SELECT required.ObjectName FROM (VALUES
    (N'lab.WallabeeVersions'), (N'lab.HoneyBadgerVersions'), (N'lab.TestHosts'), (N'lab.Datasets'),
    (N'lab.Experiments'), (N'lab.ExperimentRounds'), (N'lab.Samples'), (N'lab.Scans'),
    (N'lab.IndicatorDefinitions'), (N'lab.ScanIndicators'), (N'benchmark.BenchmarkRuns'),
    (N'telemetry.SystemMetricSamples'), (N'telemetry.TelemetryEvents'), (N'lab.QuarantineEvents'),
    (N'lab.ExperimentEvents'), (N'benchmark.DetectionConfusionMatrix'), (N'reporting.vw_ExperimentOverview'),
    (N'reporting.vw_RoundPerformance'), (N'reporting.vw_VersionComparison'), (N'reporting.vw_HoneyBadgerVersionPerformance'),
    (N'reporting.vw_IndicatorFrequency'), (N'reporting.vw_DetectionLevelDistribution'), (N'reporting.vw_LatencyTrend'),
    (N'reporting.vw_ExperimentTimeline'), (N'reporting.usp_GetExperimentDashboard'), (N'reporting.usp_GetVersionComparison'),
    (N'reporting.usp_GetRoundDetails'), (N'reporting.usp_GetFalseNegatives'), (N'reporting.usp_GetFalsePositives'),
    (N'reporting.usp_GetPerformanceTrend'), (N'reporting.usp_GetTopIndicators')
) required (ObjectName)
WHERE OBJECT_ID(required.ObjectName) IS NULL;

IF EXISTS (SELECT 1 FROM @Missing)
BEGIN
    SELECT ObjectName AS MissingObject FROM @Missing;
    THROW 51001, 'DiveViewLab validation failed: required objects are missing.', 1;
END;

IF (SELECT COUNT(*) FROM sys.foreign_keys WHERE is_ms_shipped = 0) < 35
    THROW 51002, 'DiveViewLab validation failed: foreign keys are missing.', 1;
IF (SELECT COUNT(*) FROM sys.check_constraints WHERE parent_object_id IN (SELECT object_id FROM sys.tables WHERE schema_id IN (SCHEMA_ID(N'lab'), SCHEMA_ID(N'benchmark'), SCHEMA_ID(N'telemetry')))) < 20
    THROW 51003, 'DiveViewLab validation failed: check constraints are missing.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Scans_Experiment_Round')
    THROW 51004, 'DiveViewLab validation failed: expected scan index is missing.', 1;
IF EXISTS (SELECT 1 FROM sys.columns WHERE system_type_id = TYPE_ID(N'varbinary') AND object_id IN (SELECT object_id FROM sys.tables WHERE schema_id IN (SCHEMA_ID(N'lab'), SCHEMA_ID(N'benchmark'), SCHEMA_ID(N'telemetry'))))
    THROW 51005, 'DiveViewLab validation failed: binary content columns are prohibited.', 1;

SELECT N'Validation passed' AS Result,
       (SELECT COUNT(*) FROM sys.tables WHERE schema_id IN (SCHEMA_ID(N'lab'), SCHEMA_ID(N'benchmark'), SCHEMA_ID(N'telemetry'))) AS TablesCreated,
       (SELECT COUNT(*) FROM sys.foreign_keys WHERE is_ms_shipped = 0) AS ForeignKeysCreated,
       (SELECT COUNT(*) FROM sys.check_constraints WHERE parent_object_id IN (SELECT object_id FROM sys.tables WHERE schema_id IN (SCHEMA_ID(N'lab'), SCHEMA_ID(N'benchmark'), SCHEMA_ID(N'telemetry')))) AS CheckConstraintsCreated,
       (SELECT COUNT(*) FROM sys.views WHERE schema_id = SCHEMA_ID(N'reporting')) AS ReportingViewsCreated,
       (SELECT COUNT(*) FROM sys.procedures WHERE schema_id = SCHEMA_ID(N'reporting')) AS ProceduresCreated;
GO
