ALTER TABLE lab.Scans ADD [HoneyBadgerVersionId] int NULL;
GO
ALTER TABLE lab.Scans ADD [ExternalExperimentId] nvarchar(200) NULL;
GO
ALTER TABLE lab.Scans ADD [ExternalRoundId] nvarchar(200) NULL;
GO
ALTER TABLE lab.Scans ADD [ExternalSampleId] nvarchar(200) NULL;
GO
ALTER TABLE lab.Scans ADD [ResponseAction] varchar(30) NOT NULL CONSTRAINT DF_Scans_ResponseAction DEFAULT 'None' WITH VALUES;
GO
ALTER TABLE lab.Scans ADD [RetainSample] bit NOT NULL CONSTRAINT DF_Scans_RetainSample DEFAULT 0 WITH VALUES;
GO
ALTER TABLE lab.Scans ADD [RetainReason] varchar(50) NULL;
GO
ALTER TABLE lab.Scans ADD [TelemetryPublished] bit NOT NULL CONSTRAINT DF_Scans_TelemetryPublished DEFAULT 0 WITH VALUES;
GO
ALTER TABLE lab.ScanIndicators ADD [IndicatorId] uniqueidentifier NOT NULL CONSTRAINT DF_ScanIndicators_IndicatorId DEFAULT NEWID() WITH VALUES;
GO
ALTER TABLE lab.ScanIndicators ADD [Source] nvarchar(100) NULL;
GO
ALTER TABLE lab.ScanIndicators ADD [Name] nvarchar(200) NULL;
GO
ALTER TABLE lab.ScanIndicators ADD [Description] nvarchar(1000) NULL;
GO
ALTER TABLE lab.ScanIndicators ADD [Category] varchar(50) NULL;
GO
ALTER TABLE benchmark.BenchmarkRuns ADD [LabeledSamples] int NOT NULL CONSTRAINT DF_BenchmarkRuns_LabeledSamples DEFAULT 0 WITH VALUES;
GO
ALTER TABLE benchmark.BenchmarkRuns ADD [FalsePositiveRate] decimal(9,6) NULL;
GO
ALTER TABLE benchmark.BenchmarkRuns ADD [FalseNegativeRate] decimal(9,6) NULL;
GO
ALTER TABLE benchmark.BenchmarkRuns ADD [BytesPerSecond] decimal(18,4) NOT NULL CONSTRAINT DF_BenchmarkRuns_BytesPerSecond DEFAULT 0 WITH VALUES;
GO
ALTER TABLE benchmark.BenchmarkRuns ADD [SamplesGenerated] int NULL;
GO
ALTER TABLE benchmark.BenchmarkRuns ADD [ExternalExperimentId] nvarchar(200) NULL;
GO
ALTER TABLE benchmark.BenchmarkRuns ADD [ExternalRoundId] nvarchar(200) NULL;
GO
ALTER TABLE telemetry.SystemMetricSamples ADD [CpuPeakPercent] decimal(7,3) NULL;
GO
ALTER TABLE telemetry.SystemMetricSamples ADD [MemoryPeakMb] decimal(18,3) NULL;
GO
ALTER TABLE telemetry.TelemetryEvents ADD [State] varchar(20) NOT NULL CONSTRAINT DF_TelemetryEvents_State DEFAULT 'Published' WITH VALUES;
GO
ALTER TABLE telemetry.TelemetryEvents ADD [Attempts] int NOT NULL CONSTRAINT DF_TelemetryEvents_Attempts DEFAULT 0 WITH VALUES;
GO
ALTER TABLE telemetry.TelemetryEvents ADD [NextAttemptUtc] datetime2(3) NOT NULL CONSTRAINT DF_TelemetryEvents_NextAttemptUtc DEFAULT SYSUTCDATETIME() WITH VALUES;
GO
ALTER TABLE telemetry.TelemetryEvents ADD [LastError] nvarchar(200) NULL;
GO
ALTER TABLE lab.QuarantineEvents ADD [OriginalPath] nvarchar(2048) NULL;
GO
ALTER TABLE lab.QuarantineEvents ADD [QuarantinePath] nvarchar(2048) NULL;
GO
ALTER TABLE lab.QuarantineEvents ADD [RestoredAt] datetime2(3) NULL;
GO
ALTER TABLE lab.QuarantineEvents ADD [State] varchar(20) NOT NULL CONSTRAINT DF_QuarantineEvents_State DEFAULT 'Quarantined' WITH VALUES;
GO
ALTER TABLE lab.Experiments ALTER COLUMN ExternalExperimentId nvarchar(200) NULL;
GO
ALTER TABLE lab.ExperimentRounds ALTER COLUMN ExternalRoundId nvarchar(200) NULL;
GO
ALTER TABLE lab.Samples ALTER COLUMN ExternalSampleId nvarchar(200) NULL;
GO
ALTER TABLE lab.Samples ALTER COLUMN ExperimentId uniqueidentifier NULL;
GO
ALTER TABLE lab.QuarantineEvents ALTER COLUMN ScanId uniqueidentifier NULL;
GO
IF EXISTS(SELECT 1 FROM telemetry.TelemetryEvents WHERE DATALENGTH(PayloadJson)>8000) THROW 51000, 'Historical telemetry payloads exceed 4000 characters; archive before upgrading.', 1;
GO
ALTER TABLE telemetry.TelemetryEvents DROP CONSTRAINT CK_TelemetryEvents_PayloadJson;
ALTER TABLE telemetry.TelemetryEvents ALTER COLUMN PayloadJson nvarchar(4000) NULL;
ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT CK_TelemetryEvents_PayloadJson CHECK (PayloadJson IS NULL OR ISJSON(PayloadJson)=1);
GO
CREATE TABLE telemetry.TelemetryFailures (
 Id bigint IDENTITY PRIMARY KEY, EventId uniqueidentifier NOT NULL, TimestampUtc datetime2(3) NOT NULL,
 Error nvarchar(200) NOT NULL, Attempt int NOT NULL,
 CONSTRAINT FK_TelemetryFailures_Events FOREIGN KEY(EventId) REFERENCES telemetry.TelemetryEvents(EventId));
GO
ALTER TABLE lab.Scans ADD CONSTRAINT FK_Scans_HoneyBadgerVersion FOREIGN KEY(HoneyBadgerVersionId) REFERENCES lab.HoneyBadgerVersions(HoneyBadgerVersionId);
GO
CREATE UNIQUE INDEX IX_Experiments_ExternalExperimentId ON lab.Experiments(ExternalExperimentId) WHERE ExternalExperimentId IS NOT NULL;
GO
CREATE UNIQUE INDEX IX_ExperimentRounds_ExperimentId_ExternalRoundId ON lab.ExperimentRounds(ExperimentId,ExternalRoundId) WHERE ExternalRoundId IS NOT NULL;
GO
CREATE UNIQUE INDEX IX_Samples_ExperimentId_RoundId_ExternalSampleId ON lab.Samples(ExperimentId,RoundId,ExternalSampleId) WHERE ExternalSampleId IS NOT NULL;
GO
CREATE UNIQUE INDEX IX_ScanIndicators_IndicatorId ON lab.ScanIndicators(IndicatorId);
GO
CREATE INDEX IX_Scans_StartedAt ON lab.Scans(StartedAt);
GO
CREATE INDEX IX_Scans_DetectionLevel ON lab.Scans(DetectionLevel);
GO
CREATE INDEX IX_Scans_WasDetected ON lab.Scans(WasDetected);
GO
CREATE INDEX IX_Scans_RoundId ON lab.Scans(RoundId);
GO
CREATE INDEX IX_TelemetryEvents_State_NextAttemptUtc ON telemetry.TelemetryEvents(State,NextAttemptUtc);
GO
CREATE INDEX IX_TelemetryFailures_EventId ON telemetry.TelemetryFailures(EventId);
GO
UPDATE lab.QuarantineEvents SET State=Action;
GO
UPDATE s SET ExternalExperimentId=e.ExternalExperimentId, ExternalRoundId=r.ExternalRoundId, ExternalSampleId=p.ExternalSampleId
FROM lab.Scans s LEFT JOIN lab.Experiments e ON e.ExperimentId=s.ExperimentId LEFT JOIN lab.ExperimentRounds r ON r.RoundId=s.RoundId LEFT JOIN lab.Samples p ON p.SampleId=s.SampleId;
GO
UPDATE b SET ExternalExperimentId=e.ExternalExperimentId, ExternalRoundId=r.ExternalRoundId
FROM benchmark.BenchmarkRuns b LEFT JOIN lab.Experiments e ON e.ExperimentId=b.ExperimentId LEFT JOIN lab.ExperimentRounds r ON r.RoundId=b.RoundId;
