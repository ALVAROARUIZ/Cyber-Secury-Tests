USE [DiveViewLab];
GO

ALTER TABLE lab.WallabeeVersions ADD CONSTRAINT UQ_WallabeeVersions_Version UNIQUE (Version);
ALTER TABLE lab.HoneyBadgerVersions ADD CONSTRAINT UQ_HoneyBadgerVersions_Version UNIQUE (Version);
ALTER TABLE lab.IndicatorDefinitions ADD CONSTRAINT UQ_IndicatorDefinitions_Code UNIQUE (IndicatorCode);
ALTER TABLE lab.ExperimentRounds ADD CONSTRAINT UQ_ExperimentRounds_Experiment_Round UNIQUE (ExperimentId, RoundNumber);
ALTER TABLE lab.ScanIndicators ADD CONSTRAINT UQ_ScanIndicators_Scan_Indicator UNIQUE (ScanId, IndicatorDefinitionId);
ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT UQ_TelemetryEvents_EventId UNIQUE (EventId);
GO

ALTER TABLE lab.Experiments ADD CONSTRAINT FK_Experiments_WallabeeVersion FOREIGN KEY (WallabeeVersionId) REFERENCES lab.WallabeeVersions (WallabeeVersionId);
ALTER TABLE lab.Experiments ADD CONSTRAINT FK_Experiments_HoneyBadgerVersion FOREIGN KEY (HoneyBadgerVersionId) REFERENCES lab.HoneyBadgerVersions (HoneyBadgerVersionId);
ALTER TABLE lab.Experiments ADD CONSTRAINT FK_Experiments_Dataset FOREIGN KEY (DatasetId) REFERENCES lab.Datasets (DatasetId);
ALTER TABLE lab.Experiments ADD CONSTRAINT FK_Experiments_TestHost FOREIGN KEY (TestHostId) REFERENCES lab.TestHosts (TestHostId);
ALTER TABLE lab.ExperimentRounds ADD CONSTRAINT FK_ExperimentRounds_Experiment FOREIGN KEY (ExperimentId) REFERENCES lab.Experiments (ExperimentId);
ALTER TABLE lab.Samples ADD CONSTRAINT FK_Samples_Experiment FOREIGN KEY (ExperimentId) REFERENCES lab.Experiments (ExperimentId);
ALTER TABLE lab.Samples ADD CONSTRAINT FK_Samples_Round FOREIGN KEY (RoundId) REFERENCES lab.ExperimentRounds (RoundId);
ALTER TABLE lab.Samples ADD CONSTRAINT FK_Samples_ParentSample FOREIGN KEY (ParentSampleId) REFERENCES lab.Samples (SampleId);
ALTER TABLE lab.Scans ADD CONSTRAINT FK_Scans_Sample FOREIGN KEY (SampleId) REFERENCES lab.Samples (SampleId);
ALTER TABLE lab.Scans ADD CONSTRAINT FK_Scans_Experiment FOREIGN KEY (ExperimentId) REFERENCES lab.Experiments (ExperimentId);
ALTER TABLE lab.Scans ADD CONSTRAINT FK_Scans_Round FOREIGN KEY (RoundId) REFERENCES lab.ExperimentRounds (RoundId);
ALTER TABLE lab.Scans ADD CONSTRAINT FK_Scans_WallabeeVersion FOREIGN KEY (WallabeeVersionId) REFERENCES lab.WallabeeVersions (WallabeeVersionId);
ALTER TABLE lab.Scans ADD CONSTRAINT FK_Scans_TestHost FOREIGN KEY (TestHostId) REFERENCES lab.TestHosts (TestHostId);
ALTER TABLE lab.ScanIndicators ADD CONSTRAINT FK_ScanIndicators_Scan FOREIGN KEY (ScanId) REFERENCES lab.Scans (ScanId);
ALTER TABLE lab.ScanIndicators ADD CONSTRAINT FK_ScanIndicators_Definition FOREIGN KEY (IndicatorDefinitionId) REFERENCES lab.IndicatorDefinitions (IndicatorDefinitionId);
ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT FK_BenchmarkRuns_Experiment FOREIGN KEY (ExperimentId) REFERENCES lab.Experiments (ExperimentId);
ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT FK_BenchmarkRuns_Round FOREIGN KEY (RoundId) REFERENCES lab.ExperimentRounds (RoundId);
ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT FK_BenchmarkRuns_WallabeeVersion FOREIGN KEY (WallabeeVersionId) REFERENCES lab.WallabeeVersions (WallabeeVersionId);
ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT FK_BenchmarkRuns_HoneyBadgerVersion FOREIGN KEY (HoneyBadgerVersionId) REFERENCES lab.HoneyBadgerVersions (HoneyBadgerVersionId);
ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT FK_BenchmarkRuns_TestHost FOREIGN KEY (TestHostId) REFERENCES lab.TestHosts (TestHostId);
ALTER TABLE telemetry.SystemMetricSamples ADD CONSTRAINT FK_SystemMetricSamples_Experiment FOREIGN KEY (ExperimentId) REFERENCES lab.Experiments (ExperimentId);
ALTER TABLE telemetry.SystemMetricSamples ADD CONSTRAINT FK_SystemMetricSamples_Round FOREIGN KEY (RoundId) REFERENCES lab.ExperimentRounds (RoundId);
ALTER TABLE telemetry.SystemMetricSamples ADD CONSTRAINT FK_SystemMetricSamples_Benchmark FOREIGN KEY (BenchmarkId) REFERENCES benchmark.BenchmarkRuns (BenchmarkId);
ALTER TABLE telemetry.SystemMetricSamples ADD CONSTRAINT FK_SystemMetricSamples_TestHost FOREIGN KEY (TestHostId) REFERENCES lab.TestHosts (TestHostId);
ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT FK_TelemetryEvents_Experiment FOREIGN KEY (ExperimentId) REFERENCES lab.Experiments (ExperimentId);
ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT FK_TelemetryEvents_Round FOREIGN KEY (RoundId) REFERENCES lab.ExperimentRounds (RoundId);
ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT FK_TelemetryEvents_Sample FOREIGN KEY (SampleId) REFERENCES lab.Samples (SampleId);
ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT FK_TelemetryEvents_Scan FOREIGN KEY (ScanId) REFERENCES lab.Scans (ScanId);
ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT FK_TelemetryEvents_Benchmark FOREIGN KEY (BenchmarkId) REFERENCES benchmark.BenchmarkRuns (BenchmarkId);
ALTER TABLE lab.QuarantineEvents ADD CONSTRAINT FK_QuarantineEvents_Scan FOREIGN KEY (ScanId) REFERENCES lab.Scans (ScanId);
ALTER TABLE lab.QuarantineEvents ADD CONSTRAINT FK_QuarantineEvents_Sample FOREIGN KEY (SampleId) REFERENCES lab.Samples (SampleId);
ALTER TABLE lab.ExperimentEvents ADD CONSTRAINT FK_ExperimentEvents_Experiment FOREIGN KEY (ExperimentId) REFERENCES lab.Experiments (ExperimentId);
ALTER TABLE lab.ExperimentEvents ADD CONSTRAINT FK_ExperimentEvents_Round FOREIGN KEY (RoundId) REFERENCES lab.ExperimentRounds (RoundId);
ALTER TABLE benchmark.DetectionConfusionMatrix ADD CONSTRAINT FK_DetectionConfusionMatrix_Experiment FOREIGN KEY (ExperimentId) REFERENCES lab.Experiments (ExperimentId);
ALTER TABLE benchmark.DetectionConfusionMatrix ADD CONSTRAINT FK_DetectionConfusionMatrix_Round FOREIGN KEY (RoundId) REFERENCES lab.ExperimentRounds (RoundId);
ALTER TABLE benchmark.DetectionConfusionMatrix ADD CONSTRAINT FK_DetectionConfusionMatrix_Benchmark FOREIGN KEY (BenchmarkId) REFERENCES benchmark.BenchmarkRuns (BenchmarkId);
GO

ALTER TABLE lab.Experiments ADD CONSTRAINT CK_Experiments_Status CHECK (Status IN ('Created', 'Running', 'Completed', 'Failed', 'Cancelled'));
ALTER TABLE lab.Experiments ADD CONSTRAINT CK_Experiments_ConfigurationJson CHECK (ConfigurationJson IS NULL OR ISJSON(ConfigurationJson) = 1);
ALTER TABLE lab.ExperimentRounds ADD CONSTRAINT CK_ExperimentRounds_Counts CHECK (SamplesGenerated >= 0 AND SamplesScanned >= 0 AND SamplesDetected >= 0 AND SamplesMissed >= 0 AND SamplesDetected + SamplesMissed <= SamplesScanned);
ALTER TABLE lab.ExperimentRounds ADD CONSTRAINT CK_ExperimentRounds_Scores CHECK ((DetectionRate IS NULL OR DetectionRate BETWEEN 0 AND 1) AND (EvasionRate IS NULL OR EvasionRate BETWEEN 0 AND 1) AND (PrecisionScore IS NULL OR PrecisionScore BETWEEN 0 AND 1) AND (RecallScore IS NULL OR RecallScore BETWEEN 0 AND 1) AND (F1Score IS NULL OR F1Score BETWEEN 0 AND 1));
ALTER TABLE lab.ExperimentRounds ADD CONSTRAINT CK_ExperimentRounds_Resources CHECK ((CpuAveragePercent IS NULL OR CpuAveragePercent BETWEEN 0 AND 100) AND (CpuPeakPercent IS NULL OR CpuPeakPercent BETWEEN 0 AND 100) AND (AverageScanMs IS NULL OR AverageScanMs >= 0) AND (P50ScanMs IS NULL OR P50ScanMs >= 0) AND (P95ScanMs IS NULL OR P95ScanMs >= 0) AND (P99ScanMs IS NULL OR P99ScanMs >= 0));
ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_SizeBytes CHECK (SizeBytes >= 0);
ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_Entropy CHECK (Entropy IS NULL OR Entropy BETWEEN 0 AND 8);
ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_GroundTruthLabel CHECK (GroundTruthLabel IS NULL OR GroundTruthLabel IN ('Benign', 'Malicious', 'Unknown'));
ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_RetainReason CHECK (RetainReason IS NULL OR RetainReason IN ('Regression', 'FalseNegative', 'FalsePositive', 'Research', 'Other'));
ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_MutationParametersJson CHECK (MutationParametersJson IS NULL OR ISJSON(MutationParametersJson) = 1);
ALTER TABLE lab.Scans ADD CONSTRAINT CK_Scans_FileSizeBytes CHECK (FileSizeBytes >= 0);
ALTER TABLE lab.Scans ADD CONSTRAINT CK_Scans_RiskScore CHECK (RiskScore BETWEEN 0 AND 100);
ALTER TABLE lab.Scans ADD CONSTRAINT CK_Scans_DetectionLevel CHECK (DetectionLevel IN ('Clean', 'Low', 'Medium', 'High', 'Critical'));
ALTER TABLE lab.Scans ADD CONSTRAINT CK_Scans_DurationMs CHECK (DurationMs >= 0);
ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT CK_BenchmarkRuns_Counts CHECK (SamplesScanned >= 0 AND (SamplesDetected IS NULL OR SamplesDetected >= 0) AND (SamplesMissed IS NULL OR SamplesMissed >= 0) AND (SamplesDetected IS NULL OR SamplesMissed IS NULL OR SamplesDetected + SamplesMissed <= SamplesScanned));
ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT CK_BenchmarkRuns_Scores CHECK ((DetectionRate IS NULL OR DetectionRate BETWEEN 0 AND 1) AND (EvasionRate IS NULL OR EvasionRate BETWEEN 0 AND 1) AND (PrecisionScore IS NULL OR PrecisionScore BETWEEN 0 AND 1) AND (RecallScore IS NULL OR RecallScore BETWEEN 0 AND 1) AND (F1Score IS NULL OR F1Score BETWEEN 0 AND 1));
ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT CK_BenchmarkRuns_Resources CHECK ((DurationMs IS NULL OR DurationMs >= 0) AND (CpuAveragePercent IS NULL OR CpuAveragePercent BETWEEN 0 AND 100) AND (CpuPeakPercent IS NULL OR CpuPeakPercent BETWEEN 0 AND 100));
ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT CK_BenchmarkRuns_ConfigurationJson CHECK (ConfigurationJson IS NULL OR ISJSON(ConfigurationJson) = 1);
ALTER TABLE telemetry.SystemMetricSamples ADD CONSTRAINT CK_SystemMetricSamples_Values CHECK ((CpuPercent IS NULL OR CpuPercent BETWEEN 0 AND 100) AND (MemoryPercent IS NULL OR MemoryPercent BETWEEN 0 AND 100) AND (ProcessCpuPercent IS NULL OR ProcessCpuPercent BETWEEN 0 AND 100) AND (MemoryUsedMb IS NULL OR MemoryUsedMb >= 0) AND (ProcessMemoryMb IS NULL OR ProcessMemoryMb >= 0) AND (DiskReadBytesPerSecond IS NULL OR DiskReadBytesPerSecond >= 0) AND (DiskWriteBytesPerSecond IS NULL OR DiskWriteBytesPerSecond >= 0));
ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT CK_TelemetryEvents_Severity CHECK (Severity IS NULL OR Severity IN ('Debug', 'Information', 'Warning', 'Error', 'Critical'));
ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT CK_TelemetryEvents_PayloadJson CHECK (PayloadJson IS NULL OR ISJSON(PayloadJson) = 1);
ALTER TABLE lab.QuarantineEvents ADD CONSTRAINT CK_QuarantineEvents_Action CHECK (Action IN ('Quarantined', 'Restored', 'Deleted', 'Failed'));
ALTER TABLE lab.ExperimentEvents ADD CONSTRAINT CK_ExperimentEvents_MetadataJson CHECK (MetadataJson IS NULL OR ISJSON(MetadataJson) = 1);
ALTER TABLE benchmark.DetectionConfusionMatrix ADD CONSTRAINT CK_DetectionConfusionMatrix_Counts CHECK (TruePositives >= 0 AND TrueNegatives >= 0 AND FalsePositives >= 0 AND FalseNegatives >= 0);
ALTER TABLE benchmark.DetectionConfusionMatrix ADD CONSTRAINT CK_DetectionConfusionMatrix_Scores CHECK ((PrecisionScore IS NULL OR PrecisionScore BETWEEN 0 AND 1) AND (RecallScore IS NULL OR RecallScore BETWEEN 0 AND 1) AND (SpecificityScore IS NULL OR SpecificityScore BETWEEN 0 AND 1) AND (F1Score IS NULL OR F1Score BETWEEN 0 AND 1) AND (AccuracyScore IS NULL OR AccuracyScore BETWEEN 0 AND 1));
GO
