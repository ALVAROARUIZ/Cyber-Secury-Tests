USE [DiveViewLab];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
GO

CREATE INDEX IX_Experiments_StartedAt ON lab.Experiments (StartedAt DESC) INCLUDE (Status, WallabeeVersionId, HoneyBadgerVersionId, DatasetId, TestHostId);
CREATE INDEX IX_Experiments_WallabeeVersion_Status ON lab.Experiments (WallabeeVersionId, Status) INCLUDE (StartedAt, CompletedAt);
CREATE INDEX IX_ExperimentRounds_Experiment_Round ON lab.ExperimentRounds (ExperimentId, RoundNumber) INCLUDE (GenerationNumber, StartedAt, CompletedAt);
CREATE INDEX IX_ExperimentRounds_Experiment_Generation_StartedAt ON lab.ExperimentRounds (ExperimentId, GenerationNumber, StartedAt);
CREATE INDEX IX_Samples_Sha256 ON lab.Samples (Sha256);
CREATE INDEX IX_Samples_Experiment_Round ON lab.Samples (ExperimentId, RoundId) INCLUDE (GroundTruthLabel, GenerationNumber, SampleFamily, RetainSample);
CREATE INDEX IX_Samples_Generation_Family ON lab.Samples (GenerationNumber, SampleFamily) INCLUDE (ExperimentId, RoundId);
CREATE INDEX IX_Samples_Retained ON lab.Samples (RetainSample) INCLUDE (ExperimentId, RoundId, RetainReason, Sha256) WHERE RetainSample = 1;
CREATE INDEX IX_Scans_Sha256 ON lab.Scans (Sha256);
CREATE INDEX IX_Scans_Sample ON lab.Scans (SampleId) INCLUDE (ExperimentId, RoundId, WasDetected, RiskScore, DurationMs);
CREATE INDEX IX_Scans_Experiment_Round ON lab.Scans (ExperimentId, RoundId) INCLUDE (WasDetected, RiskScore, DurationMs, DetectionLevel);
CREATE INDEX IX_Scans_Wallabee_StartedAt ON lab.Scans (WallabeeVersionId, StartedAt) INCLUDE (DurationMs, WasDetected, RiskScore, DetectionLevel);
CREATE INDEX IX_Scans_Wallabee_DetectionLevel ON lab.Scans (WallabeeVersionId, DetectionLevel) INCLUDE (StartedAt, WasDetected);
CREATE INDEX IX_Scans_Experiment_Detected ON lab.Scans (ExperimentId, WasDetected) INCLUDE (RoundId, RiskScore, DurationMs);
CREATE INDEX IX_ScanIndicators_Scan_Triggered ON lab.ScanIndicators (ScanId, Triggered) INCLUDE (IndicatorDefinitionId, WeightApplied);
CREATE INDEX IX_ScanIndicators_Definition_Triggered ON lab.ScanIndicators (IndicatorDefinitionId, Triggered) INCLUDE (ScanId, WeightApplied);
CREATE INDEX IX_BenchmarkRuns_Wallabee_StartedAt ON benchmark.BenchmarkRuns (WallabeeVersionId, StartedAt) INCLUDE (DetectionRate, EvasionRate, AverageScanMs, P95ScanMs);
CREATE INDEX IX_BenchmarkRuns_Experiment_Round ON benchmark.BenchmarkRuns (ExperimentId, RoundId) INCLUDE (StartedAt, BenchmarkType);
CREATE INDEX IX_SystemMetricSamples_Benchmark_Timestamp ON telemetry.SystemMetricSamples (BenchmarkId, TimestampUtc) INCLUDE (CpuPercent, MemoryUsedMb, ProcessCpuPercent, ProcessMemoryMb);
CREATE INDEX IX_SystemMetricSamples_Experiment_Round_Timestamp ON telemetry.SystemMetricSamples (ExperimentId, RoundId, TimestampUtc) INCLUDE (CpuPercent, MemoryUsedMb);
CREATE INDEX IX_TelemetryEvents_Experiment_Timestamp ON telemetry.TelemetryEvents (ExperimentId, TimestampUtc) INCLUDE (EventType, Severity, SourceComponent);
CREATE INDEX IX_TelemetryEvents_Source_Event_Timestamp ON telemetry.TelemetryEvents (SourceComponent, EventType, TimestampUtc) INCLUDE (Severity, ExperimentId, RoundId);
CREATE INDEX IX_ExperimentEvents_Experiment_OccurredAt ON lab.ExperimentEvents (ExperimentId, OccurredAt) INCLUDE (RoundId, EventType);
GO