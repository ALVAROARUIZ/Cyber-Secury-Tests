USE [DiveViewLab];
GO

IF DATABASE_PRINCIPAL_ID(N'DiveViewReader') IS NULL CREATE ROLE [DiveViewReader];
IF DATABASE_PRINCIPAL_ID(N'DiveTelemetryWriter') IS NULL CREATE ROLE [DiveTelemetryWriter];
IF DATABASE_PRINCIPAL_ID(N'DiveAdmin') IS NULL CREATE ROLE [DiveAdmin];
GO

GRANT SELECT ON SCHEMA::reporting TO [DiveViewReader];
GRANT EXECUTE ON SCHEMA::reporting TO [DiveViewReader];
GRANT INSERT, UPDATE ON telemetry.SystemMetricSamples TO [DiveTelemetryWriter];
GRANT INSERT ON telemetry.TelemetryEvents TO [DiveTelemetryWriter];
GRANT INSERT, UPDATE ON lab.Experiments TO [DiveTelemetryWriter];
GRANT INSERT, UPDATE ON lab.ExperimentRounds TO [DiveTelemetryWriter];
GRANT INSERT, UPDATE ON lab.Samples TO [DiveTelemetryWriter];
GRANT INSERT, UPDATE ON lab.Scans TO [DiveTelemetryWriter];
GRANT INSERT ON lab.ScanIndicators TO [DiveTelemetryWriter];
GRANT INSERT ON lab.ExperimentEvents TO [DiveTelemetryWriter];
GRANT INSERT ON lab.QuarantineEvents TO [DiveTelemetryWriter];
GRANT INSERT, UPDATE ON benchmark.BenchmarkRuns TO [DiveTelemetryWriter];
GRANT INSERT, UPDATE ON benchmark.DetectionConfusionMatrix TO [DiveTelemetryWriter];
GRANT CONTROL ON DATABASE::DiveViewLab TO [DiveAdmin];
GO