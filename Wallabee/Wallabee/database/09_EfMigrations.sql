IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN
    IF OBJECT_ID(N'lab.Scans', N'U') IS NULL BEGIN
    EXEC(N'/* Run after 00_CreateDatabase.sql. */

    ');
    EXEC(N'
    IF SCHEMA_ID(N''lab'') IS NULL
        EXECUTE(N''CREATE SCHEMA [lab] AUTHORIZATION [dbo];'');
    ');
    EXEC(N'
    IF SCHEMA_ID(N''benchmark'') IS NULL
        EXECUTE(N''CREATE SCHEMA [benchmark] AUTHORIZATION [dbo];'');
    ');
    EXEC(N'
    IF SCHEMA_ID(N''telemetry'') IS NULL
        EXECUTE(N''CREATE SCHEMA [telemetry] AUTHORIZATION [dbo];'');
    ');
    EXEC(N'
    IF SCHEMA_ID(N''reporting'') IS NULL
        EXECUTE(N''CREATE SCHEMA [reporting] AUTHORIZATION [dbo];'');
    ');
    EXEC(N'
    CREATE TABLE lab.WallabeeVersions (
        WallabeeVersionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WallabeeVersions PRIMARY KEY,
        Version VARCHAR(50) NOT NULL, MajorVersion INT NULL, MinorVersion INT NULL, PatchVersion INT NULL,
        BuildMetadata VARCHAR(100) NULL, CommitHash VARCHAR(100) NULL, BranchName VARCHAR(100) NULL,
        BuildDate DATETIME2(3) NULL, Description NVARCHAR(500) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_WallabeeVersions_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE lab.HoneyBadgerVersions (
        HoneyBadgerVersionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoneyBadgerVersions PRIMARY KEY,
        Version VARCHAR(50) NOT NULL, MajorVersion INT NULL, MinorVersion INT NULL, PatchVersion INT NULL,
        CommitHash VARCHAR(100) NULL, BranchName VARCHAR(100) NULL, BuildDate DATETIME2(3) NULL,
        Description NVARCHAR(500) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_HoneyBadgerVersions_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE lab.TestHosts (
        TestHostId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TestHosts PRIMARY KEY,
        HostName NVARCHAR(150) NOT NULL, HostAlias NVARCHAR(150) NULL, OperatingSystem NVARCHAR(150) NULL,
        OperatingSystemVersion NVARCHAR(100) NULL, Architecture VARCHAR(50) NULL, CpuModel NVARCHAR(200) NULL,
        CpuLogicalCores INT NULL, CpuPhysicalCores INT NULL, TotalMemoryMb BIGINT NULL, GpuModel NVARCHAR(200) NULL,
        StorageType NVARCHAR(100) NULL, IsVirtualMachine BIT NOT NULL CONSTRAINT DF_TestHosts_IsVirtualMachine DEFAULT 0,
        Hypervisor NVARCHAR(100) NULL, EnvironmentName NVARCHAR(100) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_TestHosts_CreatedAt DEFAULT SYSUTCDATETIME(),
        LastSeenAt DATETIME2(3) NULL, IsActive BIT NOT NULL CONSTRAINT DF_TestHosts_IsActive DEFAULT 1
    );
    ');
    EXEC(N'
    CREATE TABLE lab.Datasets (
        DatasetId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Datasets PRIMARY KEY CONSTRAINT DF_Datasets_Id DEFAULT NEWSEQUENTIALID(),
        DatasetName NVARCHAR(200) NOT NULL, DatasetVersion VARCHAR(50) NULL, Description NVARCHAR(1000) NULL,
        Seed BIGINT NULL, TotalSamples INT NULL, BenignSamples INT NULL, SyntheticSamples INT NULL,
        ManifestSha256 CHAR(64) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Datasets_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE lab.Experiments (
        ExperimentId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Experiments PRIMARY KEY CONSTRAINT DF_Experiments_Id DEFAULT NEWSEQUENTIALID(),
        ExternalExperimentId VARCHAR(100) NULL, ExperimentName NVARCHAR(200) NOT NULL, Description NVARCHAR(1000) NULL,
        WallabeeVersionId INT NOT NULL, HoneyBadgerVersionId INT NULL, DatasetId UNIQUEIDENTIFIER NULL, TestHostId INT NULL,
        ExperimentType VARCHAR(50) NULL, Status VARCHAR(30) NOT NULL, StartedAt DATETIME2(3) NOT NULL,
        CompletedAt DATETIME2(3) NULL, RandomSeed BIGINT NULL, TotalRounds INT NULL, TotalSamples INT NULL,
        ConfigurationJson NVARCHAR(MAX) NULL, IsArchived BIT NOT NULL CONSTRAINT DF_Experiments_IsArchived DEFAULT 0,
        ArchivedAt DATETIME2(3) NULL, CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Experiments_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE lab.ExperimentRounds (
        RoundId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ExperimentRounds PRIMARY KEY CONSTRAINT DF_ExperimentRounds_Id DEFAULT NEWSEQUENTIALID(),
        ExperimentId UNIQUEIDENTIFIER NOT NULL, ExternalRoundId VARCHAR(100) NULL, RoundNumber INT NOT NULL,
        GenerationNumber INT NULL, StartedAt DATETIME2(3) NOT NULL, CompletedAt DATETIME2(3) NULL,
        SamplesGenerated INT NOT NULL CONSTRAINT DF_ExperimentRounds_SamplesGenerated DEFAULT 0,
        SamplesScanned INT NOT NULL CONSTRAINT DF_ExperimentRounds_SamplesScanned DEFAULT 0,
        SamplesDetected INT NOT NULL CONSTRAINT DF_ExperimentRounds_SamplesDetected DEFAULT 0,
        SamplesMissed INT NOT NULL CONSTRAINT DF_ExperimentRounds_SamplesMissed DEFAULT 0,
        TruePositives INT NULL, TrueNegatives INT NULL, FalsePositives INT NULL, FalseNegatives INT NULL,
        DetectionRate DECIMAL(9,6) NULL, EvasionRate DECIMAL(9,6) NULL, PrecisionScore DECIMAL(9,6) NULL,
        RecallScore DECIMAL(9,6) NULL, F1Score DECIMAL(9,6) NULL, AverageRiskScore DECIMAL(9,4) NULL,
        AverageScanMs DECIMAL(18,6) NULL, P50ScanMs DECIMAL(18,6) NULL, P95ScanMs DECIMAL(18,6) NULL,
        P99ScanMs DECIMAL(18,6) NULL, FilesPerSecond DECIMAL(18,4) NULL, MbPerSecond DECIMAL(18,4) NULL,
        CpuAveragePercent DECIMAL(7,3) NULL, CpuPeakPercent DECIMAL(7,3) NULL,
        MemoryAverageMb DECIMAL(18,3) NULL, MemoryPeakMb DECIMAL(18,3) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_ExperimentRounds_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE lab.Samples (
        SampleId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Samples PRIMARY KEY CONSTRAINT DF_Samples_Id DEFAULT NEWSEQUENTIALID(),
        ExternalSampleId VARCHAR(150) NULL, ExperimentId UNIQUEIDENTIFIER NOT NULL, RoundId UNIQUEIDENTIFIER NULL,
        ParentSampleId UNIQUEIDENTIFIER NULL, SampleFamily NVARCHAR(100) NULL, SampleType NVARCHAR(100) NULL,
        GroundTruthLabel VARCHAR(30) NULL, GenerationNumber INT NULL, FileName NVARCHAR(260) NULL,
        RelativePath NVARCHAR(500) NULL, Sha256 CHAR(64) NOT NULL, SizeBytes BIGINT NOT NULL, Entropy DECIMAL(9,6) NULL,
        MutationStrategy NVARCHAR(150) NULL, MutationParametersJson NVARCHAR(MAX) NULL, FitnessScore DECIMAL(9,6) NULL,
        RetainSample BIT NOT NULL CONSTRAINT DF_Samples_RetainSample DEFAULT 0, RetainReason VARCHAR(50) NULL,
        ExternalStorageReference NVARCHAR(500) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Samples_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE lab.Scans (
        ScanId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Scans PRIMARY KEY CONSTRAINT DF_Scans_Id DEFAULT NEWSEQUENTIALID(),
        SampleId UNIQUEIDENTIFIER NULL, ExperimentId UNIQUEIDENTIFIER NULL, RoundId UNIQUEIDENTIFIER NULL,
        WallabeeVersionId INT NOT NULL, TestHostId INT NULL, Sha256 CHAR(64) NOT NULL, FileName NVARCHAR(260) NULL,
        FileSizeBytes BIGINT NOT NULL, RiskScore DECIMAL(9,4) NOT NULL, DetectionLevel VARCHAR(30) NOT NULL,
        WasDetected BIT NOT NULL, StartedAt DATETIME2(3) NOT NULL, CompletedAt DATETIME2(3) NOT NULL,
        DurationMs DECIMAL(18,6) NOT NULL, BytesPerSecond DECIMAL(18,4) NULL,
        ErrorOccurred BIT NOT NULL CONSTRAINT DF_Scans_ErrorOccurred DEFAULT 0, ErrorCode VARCHAR(100) NULL,
        ErrorMessage NVARCHAR(1000) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Scans_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE lab.IndicatorDefinitions (
        IndicatorDefinitionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndicatorDefinitions PRIMARY KEY,
        IndicatorCode VARCHAR(100) NOT NULL, IndicatorName NVARCHAR(200) NOT NULL, Source NVARCHAR(100) NOT NULL,
        Description NVARCHAR(1000) NULL, DefaultWeight DECIMAL(9,4) NULL, Category NVARCHAR(100) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_IndicatorDefinitions_IsActive DEFAULT 1,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_IndicatorDefinitions_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE lab.ScanIndicators (
        ScanIndicatorId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScanIndicators PRIMARY KEY,
        ScanId UNIQUEIDENTIFIER NOT NULL, IndicatorDefinitionId INT NOT NULL, WeightApplied DECIMAL(9,4) NULL,
        ObservedValue NVARCHAR(500) NULL, Triggered BIT NOT NULL CONSTRAINT DF_ScanIndicators_Triggered DEFAULT 1,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_ScanIndicators_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE benchmark.BenchmarkRuns (
        BenchmarkId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_BenchmarkRuns PRIMARY KEY CONSTRAINT DF_BenchmarkRuns_Id DEFAULT NEWSEQUENTIALID(),
        ExperimentId UNIQUEIDENTIFIER NULL, RoundId UNIQUEIDENTIFIER NULL, WallabeeVersionId INT NOT NULL,
        HoneyBadgerVersionId INT NULL, TestHostId INT NULL, BenchmarkType VARCHAR(100) NOT NULL,
        StartedAt DATETIME2(3) NOT NULL, CompletedAt DATETIME2(3) NULL, DurationMs DECIMAL(18,6) NULL,
        SamplesScanned INT NOT NULL CONSTRAINT DF_BenchmarkRuns_SamplesScanned DEFAULT 0,
        SamplesDetected INT NULL, SamplesMissed INT NULL, TruePositives INT NULL, TrueNegatives INT NULL,
        FalsePositives INT NULL, FalseNegatives INT NULL, DetectionRate DECIMAL(9,6) NULL,
        EvasionRate DECIMAL(9,6) NULL, PrecisionScore DECIMAL(9,6) NULL, RecallScore DECIMAL(9,6) NULL,
        F1Score DECIMAL(9,6) NULL, AverageRiskScore DECIMAL(9,4) NULL, AverageScanMs DECIMAL(18,6) NULL,
        P50ScanMs DECIMAL(18,6) NULL, P95ScanMs DECIMAL(18,6) NULL, P99ScanMs DECIMAL(18,6) NULL,
        FilesPerSecond DECIMAL(18,4) NULL, MbPerSecond DECIMAL(18,4) NULL,
        CpuAveragePercent DECIMAL(7,3) NULL, CpuPeakPercent DECIMAL(7,3) NULL,
        MemoryAverageMb DECIMAL(18,3) NULL, MemoryPeakMb DECIMAL(18,3) NULL,
        ConfigurationJson NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_BenchmarkRuns_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE telemetry.SystemMetricSamples (
        SystemMetricSampleId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SystemMetricSamples PRIMARY KEY,
        ExperimentId UNIQUEIDENTIFIER NULL, RoundId UNIQUEIDENTIFIER NULL, BenchmarkId UNIQUEIDENTIFIER NULL,
        TestHostId INT NULL, TimestampUtc DATETIME2(3) NOT NULL, CpuPercent DECIMAL(7,3) NULL,
        MemoryUsedMb DECIMAL(18,3) NULL, MemoryPercent DECIMAL(7,3) NULL,
        DiskReadBytesPerSecond BIGINT NULL, DiskWriteBytesPerSecond BIGINT NULL,
        DiskReadOperationsPerSecond DECIMAL(18,3) NULL, DiskWriteOperationsPerSecond DECIMAL(18,3) NULL,
        ProcessCpuPercent DECIMAL(7,3) NULL, ProcessMemoryMb DECIMAL(18,3) NULL, ThreadCount INT NULL,
        HandleCount INT NULL
    );
    ');
    EXEC(N'
    CREATE TABLE telemetry.TelemetryEvents (
        TelemetryEventId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TelemetryEvents PRIMARY KEY,
        EventId UNIQUEIDENTIFIER NOT NULL, ExperimentId UNIQUEIDENTIFIER NULL, RoundId UNIQUEIDENTIFIER NULL,
        SampleId UNIQUEIDENTIFIER NULL, ScanId UNIQUEIDENTIFIER NULL, BenchmarkId UNIQUEIDENTIFIER NULL,
        SourceComponent VARCHAR(100) NOT NULL, EventType VARCHAR(150) NOT NULL, Severity VARCHAR(30) NULL,
        TimestampUtc DATETIME2(3) NOT NULL, Message NVARCHAR(1000) NULL, PayloadJson NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_TelemetryEvents_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    CREATE TABLE lab.QuarantineEvents (
        QuarantineEventId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_QuarantineEvents PRIMARY KEY CONSTRAINT DF_QuarantineEvents_Id DEFAULT NEWSEQUENTIALID(),
        ScanId UNIQUEIDENTIFIER NOT NULL, SampleId UNIQUEIDENTIFIER NULL, Sha256 CHAR(64) NOT NULL,
        Action VARCHAR(30) NOT NULL, Reason NVARCHAR(500) NULL, QuarantineReference NVARCHAR(500) NULL,
        OccurredAt DATETIME2(3) NOT NULL
    );
    ');
    EXEC(N'
    CREATE TABLE lab.ExperimentEvents (
        ExperimentEventId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExperimentEvents PRIMARY KEY,
        ExperimentId UNIQUEIDENTIFIER NOT NULL, RoundId UNIQUEIDENTIFIER NULL, EventType VARCHAR(100) NOT NULL,
        Message NVARCHAR(1000) NULL, OccurredAt DATETIME2(3) NOT NULL, MetadataJson NVARCHAR(MAX) NULL
    );
    ');
    EXEC(N'
    CREATE TABLE benchmark.DetectionConfusionMatrix (
        DetectionConfusionMatrixId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DetectionConfusionMatrix PRIMARY KEY,
        ExperimentId UNIQUEIDENTIFIER NOT NULL, RoundId UNIQUEIDENTIFIER NULL, BenchmarkId UNIQUEIDENTIFIER NULL,
        TruePositives INT NOT NULL, TrueNegatives INT NOT NULL, FalsePositives INT NOT NULL, FalseNegatives INT NOT NULL,
        PrecisionScore DECIMAL(9,6) NULL, RecallScore DECIMAL(9,6) NULL, SpecificityScore DECIMAL(9,6) NULL,
        F1Score DECIMAL(9,6) NULL, AccuracyScore DECIMAL(9,6) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_DetectionConfusionMatrix_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    ');
    EXEC(N'
    ALTER TABLE lab.WallabeeVersions ADD CONSTRAINT UQ_WallabeeVersions_Version UNIQUE (Version);
    ALTER TABLE lab.HoneyBadgerVersions ADD CONSTRAINT UQ_HoneyBadgerVersions_Version UNIQUE (Version);
    ALTER TABLE lab.IndicatorDefinitions ADD CONSTRAINT UQ_IndicatorDefinitions_Code UNIQUE (IndicatorCode);
    ALTER TABLE lab.ExperimentRounds ADD CONSTRAINT UQ_ExperimentRounds_Experiment_Round UNIQUE (ExperimentId, RoundNumber);
    ALTER TABLE lab.ScanIndicators ADD CONSTRAINT UQ_ScanIndicators_Scan_Indicator UNIQUE (ScanId, IndicatorDefinitionId);
    ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT UQ_TelemetryEvents_EventId UNIQUE (EventId);
    ');
    EXEC(N'
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
    ');
    EXEC(N'
    ALTER TABLE lab.Experiments ADD CONSTRAINT CK_Experiments_Status CHECK (Status IN (''Created'', ''Running'', ''Completed'', ''Failed'', ''Cancelled''));
    ALTER TABLE lab.Experiments ADD CONSTRAINT CK_Experiments_ConfigurationJson CHECK (ConfigurationJson IS NULL OR ISJSON(ConfigurationJson) = 1);
    ALTER TABLE lab.ExperimentRounds ADD CONSTRAINT CK_ExperimentRounds_Counts CHECK (SamplesGenerated >= 0 AND SamplesScanned >= 0 AND SamplesDetected >= 0 AND SamplesMissed >= 0 AND SamplesDetected + SamplesMissed <= SamplesScanned);
    ALTER TABLE lab.ExperimentRounds ADD CONSTRAINT CK_ExperimentRounds_Scores CHECK ((DetectionRate IS NULL OR DetectionRate BETWEEN 0 AND 1) AND (EvasionRate IS NULL OR EvasionRate BETWEEN 0 AND 1) AND (PrecisionScore IS NULL OR PrecisionScore BETWEEN 0 AND 1) AND (RecallScore IS NULL OR RecallScore BETWEEN 0 AND 1) AND (F1Score IS NULL OR F1Score BETWEEN 0 AND 1));
    ALTER TABLE lab.ExperimentRounds ADD CONSTRAINT CK_ExperimentRounds_Resources CHECK ((CpuAveragePercent IS NULL OR CpuAveragePercent BETWEEN 0 AND 100) AND (CpuPeakPercent IS NULL OR CpuPeakPercent BETWEEN 0 AND 100) AND (AverageScanMs IS NULL OR AverageScanMs >= 0) AND (P50ScanMs IS NULL OR P50ScanMs >= 0) AND (P95ScanMs IS NULL OR P95ScanMs >= 0) AND (P99ScanMs IS NULL OR P99ScanMs >= 0));
    ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_SizeBytes CHECK (SizeBytes >= 0);
    ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_Entropy CHECK (Entropy IS NULL OR Entropy BETWEEN 0 AND 8);
    ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_GroundTruthLabel CHECK (GroundTruthLabel IS NULL OR GroundTruthLabel IN (''Benign'', ''Malicious'', ''Unknown''));
    ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_RetainReason CHECK (RetainReason IS NULL OR RetainReason IN (''Regression'', ''FalseNegative'', ''FalsePositive'', ''Research'', ''Other''));
    ALTER TABLE lab.Samples ADD CONSTRAINT CK_Samples_MutationParametersJson CHECK (MutationParametersJson IS NULL OR ISJSON(MutationParametersJson) = 1);
    ALTER TABLE lab.Scans ADD CONSTRAINT CK_Scans_FileSizeBytes CHECK (FileSizeBytes >= 0);
    ALTER TABLE lab.Scans ADD CONSTRAINT CK_Scans_RiskScore CHECK (RiskScore BETWEEN 0 AND 100);
    ALTER TABLE lab.Scans ADD CONSTRAINT CK_Scans_DetectionLevel CHECK (DetectionLevel IN (''Clean'', ''Low'', ''Medium'', ''High'', ''Critical''));
    ALTER TABLE lab.Scans ADD CONSTRAINT CK_Scans_DurationMs CHECK (DurationMs >= 0);
    ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT CK_BenchmarkRuns_Counts CHECK (SamplesScanned >= 0 AND (SamplesDetected IS NULL OR SamplesDetected >= 0) AND (SamplesMissed IS NULL OR SamplesMissed >= 0) AND (SamplesDetected IS NULL OR SamplesMissed IS NULL OR SamplesDetected + SamplesMissed <= SamplesScanned));
    ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT CK_BenchmarkRuns_Scores CHECK ((DetectionRate IS NULL OR DetectionRate BETWEEN 0 AND 1) AND (EvasionRate IS NULL OR EvasionRate BETWEEN 0 AND 1) AND (PrecisionScore IS NULL OR PrecisionScore BETWEEN 0 AND 1) AND (RecallScore IS NULL OR RecallScore BETWEEN 0 AND 1) AND (F1Score IS NULL OR F1Score BETWEEN 0 AND 1));
    ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT CK_BenchmarkRuns_Resources CHECK ((DurationMs IS NULL OR DurationMs >= 0) AND (CpuAveragePercent IS NULL OR CpuAveragePercent BETWEEN 0 AND 100) AND (CpuPeakPercent IS NULL OR CpuPeakPercent BETWEEN 0 AND 100));
    ALTER TABLE benchmark.BenchmarkRuns ADD CONSTRAINT CK_BenchmarkRuns_ConfigurationJson CHECK (ConfigurationJson IS NULL OR ISJSON(ConfigurationJson) = 1);
    ALTER TABLE telemetry.SystemMetricSamples ADD CONSTRAINT CK_SystemMetricSamples_Values CHECK ((CpuPercent IS NULL OR CpuPercent BETWEEN 0 AND 100) AND (MemoryPercent IS NULL OR MemoryPercent BETWEEN 0 AND 100) AND (ProcessCpuPercent IS NULL OR ProcessCpuPercent BETWEEN 0 AND 100) AND (MemoryUsedMb IS NULL OR MemoryUsedMb >= 0) AND (ProcessMemoryMb IS NULL OR ProcessMemoryMb >= 0) AND (DiskReadBytesPerSecond IS NULL OR DiskReadBytesPerSecond >= 0) AND (DiskWriteBytesPerSecond IS NULL OR DiskWriteBytesPerSecond >= 0));
    ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT CK_TelemetryEvents_Severity CHECK (Severity IS NULL OR Severity IN (''Debug'', ''Information'', ''Warning'', ''Error'', ''Critical''));
    ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT CK_TelemetryEvents_PayloadJson CHECK (PayloadJson IS NULL OR ISJSON(PayloadJson) = 1);
    ALTER TABLE lab.QuarantineEvents ADD CONSTRAINT CK_QuarantineEvents_Action CHECK (Action IN (''Quarantined'', ''Restored'', ''Deleted'', ''Failed''));
    ALTER TABLE lab.ExperimentEvents ADD CONSTRAINT CK_ExperimentEvents_MetadataJson CHECK (MetadataJson IS NULL OR ISJSON(MetadataJson) = 1);
    ALTER TABLE benchmark.DetectionConfusionMatrix ADD CONSTRAINT CK_DetectionConfusionMatrix_Counts CHECK (TruePositives >= 0 AND TrueNegatives >= 0 AND FalsePositives >= 0 AND FalseNegatives >= 0);
    ALTER TABLE benchmark.DetectionConfusionMatrix ADD CONSTRAINT CK_DetectionConfusionMatrix_Scores CHECK ((PrecisionScore IS NULL OR PrecisionScore BETWEEN 0 AND 1) AND (RecallScore IS NULL OR RecallScore BETWEEN 0 AND 1) AND (SpecificityScore IS NULL OR SpecificityScore BETWEEN 0 AND 1) AND (F1Score IS NULL OR F1Score BETWEEN 0 AND 1) AND (AccuracyScore IS NULL OR AccuracyScore BETWEEN 0 AND 1));
    ');
    EXEC(N'
    SET ANSI_NULLS ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_PADDING ON;
    SET ANSI_WARNINGS ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    ');
    EXEC(N'
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
    ');
    END;
    IF OBJECT_ID(N'lab.WallabeeVersions', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.WallabeeVersions', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'WallabeeVersionId') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.WallabeeVersionId', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'Version') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.Version', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'MajorVersion') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.MajorVersion', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'MinorVersion') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.MinorVersion', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'PatchVersion') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.PatchVersion', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'BuildMetadata') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.BuildMetadata', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'CommitHash') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.CommitHash', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'BranchName') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.BranchName', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'BuildDate') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.BuildDate', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'Description') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.Description', 1;
    IF COL_LENGTH(N'lab.WallabeeVersions', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.WallabeeVersions.CreatedAt', 1;
    IF OBJECT_ID(N'lab.HoneyBadgerVersions', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.HoneyBadgerVersions', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'HoneyBadgerVersionId') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.HoneyBadgerVersionId', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'Version') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.Version', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'MajorVersion') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.MajorVersion', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'MinorVersion') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.MinorVersion', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'PatchVersion') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.PatchVersion', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'CommitHash') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.CommitHash', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'BranchName') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.BranchName', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'BuildDate') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.BuildDate', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'Description') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.Description', 1;
    IF COL_LENGTH(N'lab.HoneyBadgerVersions', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.HoneyBadgerVersions.CreatedAt', 1;
    IF OBJECT_ID(N'lab.TestHosts', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.TestHosts', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'TestHostId') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.TestHostId', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'HostName') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.HostName', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'HostAlias') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.HostAlias', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'OperatingSystem') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.OperatingSystem', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'OperatingSystemVersion') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.OperatingSystemVersion', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'Architecture') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.Architecture', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'CpuModel') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.CpuModel', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'CpuLogicalCores') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.CpuLogicalCores', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'CpuPhysicalCores') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.CpuPhysicalCores', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'TotalMemoryMb') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.TotalMemoryMb', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'GpuModel') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.GpuModel', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'StorageType') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.StorageType', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'IsVirtualMachine') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.IsVirtualMachine', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'Hypervisor') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.Hypervisor', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'EnvironmentName') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.EnvironmentName', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.CreatedAt', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'LastSeenAt') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.LastSeenAt', 1;
    IF COL_LENGTH(N'lab.TestHosts', N'IsActive') IS NULL THROW 51000, 'Baseline column missing: lab.TestHosts.IsActive', 1;
    IF OBJECT_ID(N'lab.Datasets', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.Datasets', 1;
    IF COL_LENGTH(N'lab.Datasets', N'DatasetId') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.DatasetId', 1;
    IF COL_LENGTH(N'lab.Datasets', N'DatasetName') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.DatasetName', 1;
    IF COL_LENGTH(N'lab.Datasets', N'DatasetVersion') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.DatasetVersion', 1;
    IF COL_LENGTH(N'lab.Datasets', N'Description') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.Description', 1;
    IF COL_LENGTH(N'lab.Datasets', N'Seed') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.Seed', 1;
    IF COL_LENGTH(N'lab.Datasets', N'TotalSamples') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.TotalSamples', 1;
    IF COL_LENGTH(N'lab.Datasets', N'BenignSamples') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.BenignSamples', 1;
    IF COL_LENGTH(N'lab.Datasets', N'SyntheticSamples') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.SyntheticSamples', 1;
    IF COL_LENGTH(N'lab.Datasets', N'ManifestSha256') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.ManifestSha256', 1;
    IF COL_LENGTH(N'lab.Datasets', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.Datasets.CreatedAt', 1;
    IF OBJECT_ID(N'lab.Experiments', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.Experiments', 1;
    IF COL_LENGTH(N'lab.Experiments', N'ExperimentId') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.ExperimentId', 1;
    IF COL_LENGTH(N'lab.Experiments', N'ExternalExperimentId') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.ExternalExperimentId', 1;
    IF COL_LENGTH(N'lab.Experiments', N'ExperimentName') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.ExperimentName', 1;
    IF COL_LENGTH(N'lab.Experiments', N'Description') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.Description', 1;
    IF COL_LENGTH(N'lab.Experiments', N'WallabeeVersionId') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.WallabeeVersionId', 1;
    IF COL_LENGTH(N'lab.Experiments', N'HoneyBadgerVersionId') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.HoneyBadgerVersionId', 1;
    IF COL_LENGTH(N'lab.Experiments', N'DatasetId') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.DatasetId', 1;
    IF COL_LENGTH(N'lab.Experiments', N'TestHostId') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.TestHostId', 1;
    IF COL_LENGTH(N'lab.Experiments', N'ExperimentType') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.ExperimentType', 1;
    IF COL_LENGTH(N'lab.Experiments', N'Status') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.Status', 1;
    IF COL_LENGTH(N'lab.Experiments', N'StartedAt') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.StartedAt', 1;
    IF COL_LENGTH(N'lab.Experiments', N'CompletedAt') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.CompletedAt', 1;
    IF COL_LENGTH(N'lab.Experiments', N'RandomSeed') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.RandomSeed', 1;
    IF COL_LENGTH(N'lab.Experiments', N'TotalRounds') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.TotalRounds', 1;
    IF COL_LENGTH(N'lab.Experiments', N'TotalSamples') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.TotalSamples', 1;
    IF COL_LENGTH(N'lab.Experiments', N'ConfigurationJson') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.ConfigurationJson', 1;
    IF COL_LENGTH(N'lab.Experiments', N'IsArchived') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.IsArchived', 1;
    IF COL_LENGTH(N'lab.Experiments', N'ArchivedAt') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.ArchivedAt', 1;
    IF COL_LENGTH(N'lab.Experiments', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.Experiments.CreatedAt', 1;
    IF OBJECT_ID(N'lab.ExperimentRounds', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.ExperimentRounds', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'RoundId') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.RoundId', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'ExperimentId') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.ExperimentId', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'ExternalRoundId') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.ExternalRoundId', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'RoundNumber') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.RoundNumber', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'GenerationNumber') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.GenerationNumber', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'StartedAt') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.StartedAt', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'CompletedAt') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.CompletedAt', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'SamplesGenerated') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.SamplesGenerated', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'SamplesScanned') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.SamplesScanned', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'SamplesDetected') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.SamplesDetected', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'SamplesMissed') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.SamplesMissed', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'TruePositives') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.TruePositives', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'TrueNegatives') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.TrueNegatives', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'FalsePositives') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.FalsePositives', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'FalseNegatives') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.FalseNegatives', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'DetectionRate') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.DetectionRate', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'EvasionRate') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.EvasionRate', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'PrecisionScore') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.PrecisionScore', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'RecallScore') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.RecallScore', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'F1Score') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.F1Score', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'AverageRiskScore') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.AverageRiskScore', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'AverageScanMs') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.AverageScanMs', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'P50ScanMs') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.P50ScanMs', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'P95ScanMs') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.P95ScanMs', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'P99ScanMs') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.P99ScanMs', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'FilesPerSecond') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.FilesPerSecond', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'MbPerSecond') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.MbPerSecond', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'CpuAveragePercent') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.CpuAveragePercent', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'CpuPeakPercent') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.CpuPeakPercent', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'MemoryAverageMb') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.MemoryAverageMb', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'MemoryPeakMb') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.MemoryPeakMb', 1;
    IF COL_LENGTH(N'lab.ExperimentRounds', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentRounds.CreatedAt', 1;
    IF OBJECT_ID(N'lab.Samples', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.Samples', 1;
    IF COL_LENGTH(N'lab.Samples', N'SampleId') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.SampleId', 1;
    IF COL_LENGTH(N'lab.Samples', N'ExternalSampleId') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.ExternalSampleId', 1;
    IF COL_LENGTH(N'lab.Samples', N'ExperimentId') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.ExperimentId', 1;
    IF COL_LENGTH(N'lab.Samples', N'RoundId') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.RoundId', 1;
    IF COL_LENGTH(N'lab.Samples', N'ParentSampleId') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.ParentSampleId', 1;
    IF COL_LENGTH(N'lab.Samples', N'SampleFamily') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.SampleFamily', 1;
    IF COL_LENGTH(N'lab.Samples', N'SampleType') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.SampleType', 1;
    IF COL_LENGTH(N'lab.Samples', N'GroundTruthLabel') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.GroundTruthLabel', 1;
    IF COL_LENGTH(N'lab.Samples', N'GenerationNumber') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.GenerationNumber', 1;
    IF COL_LENGTH(N'lab.Samples', N'FileName') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.FileName', 1;
    IF COL_LENGTH(N'lab.Samples', N'RelativePath') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.RelativePath', 1;
    IF COL_LENGTH(N'lab.Samples', N'Sha256') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.Sha256', 1;
    IF COL_LENGTH(N'lab.Samples', N'SizeBytes') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.SizeBytes', 1;
    IF COL_LENGTH(N'lab.Samples', N'Entropy') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.Entropy', 1;
    IF COL_LENGTH(N'lab.Samples', N'MutationStrategy') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.MutationStrategy', 1;
    IF COL_LENGTH(N'lab.Samples', N'MutationParametersJson') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.MutationParametersJson', 1;
    IF COL_LENGTH(N'lab.Samples', N'FitnessScore') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.FitnessScore', 1;
    IF COL_LENGTH(N'lab.Samples', N'RetainSample') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.RetainSample', 1;
    IF COL_LENGTH(N'lab.Samples', N'RetainReason') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.RetainReason', 1;
    IF COL_LENGTH(N'lab.Samples', N'ExternalStorageReference') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.ExternalStorageReference', 1;
    IF COL_LENGTH(N'lab.Samples', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.Samples.CreatedAt', 1;
    IF OBJECT_ID(N'lab.Scans', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.Scans', 1;
    IF COL_LENGTH(N'lab.Scans', N'ScanId') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.ScanId', 1;
    IF COL_LENGTH(N'lab.Scans', N'SampleId') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.SampleId', 1;
    IF COL_LENGTH(N'lab.Scans', N'ExperimentId') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.ExperimentId', 1;
    IF COL_LENGTH(N'lab.Scans', N'RoundId') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.RoundId', 1;
    IF COL_LENGTH(N'lab.Scans', N'WallabeeVersionId') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.WallabeeVersionId', 1;
    IF COL_LENGTH(N'lab.Scans', N'TestHostId') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.TestHostId', 1;
    IF COL_LENGTH(N'lab.Scans', N'Sha256') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.Sha256', 1;
    IF COL_LENGTH(N'lab.Scans', N'FileName') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.FileName', 1;
    IF COL_LENGTH(N'lab.Scans', N'FileSizeBytes') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.FileSizeBytes', 1;
    IF COL_LENGTH(N'lab.Scans', N'RiskScore') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.RiskScore', 1;
    IF COL_LENGTH(N'lab.Scans', N'DetectionLevel') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.DetectionLevel', 1;
    IF COL_LENGTH(N'lab.Scans', N'WasDetected') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.WasDetected', 1;
    IF COL_LENGTH(N'lab.Scans', N'StartedAt') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.StartedAt', 1;
    IF COL_LENGTH(N'lab.Scans', N'CompletedAt') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.CompletedAt', 1;
    IF COL_LENGTH(N'lab.Scans', N'DurationMs') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.DurationMs', 1;
    IF COL_LENGTH(N'lab.Scans', N'BytesPerSecond') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.BytesPerSecond', 1;
    IF COL_LENGTH(N'lab.Scans', N'ErrorOccurred') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.ErrorOccurred', 1;
    IF COL_LENGTH(N'lab.Scans', N'ErrorCode') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.ErrorCode', 1;
    IF COL_LENGTH(N'lab.Scans', N'ErrorMessage') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.ErrorMessage', 1;
    IF COL_LENGTH(N'lab.Scans', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.Scans.CreatedAt', 1;
    IF OBJECT_ID(N'lab.IndicatorDefinitions', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.IndicatorDefinitions', 1;
    IF COL_LENGTH(N'lab.IndicatorDefinitions', N'IndicatorDefinitionId') IS NULL THROW 51000, 'Baseline column missing: lab.IndicatorDefinitions.IndicatorDefinitionId', 1;
    IF COL_LENGTH(N'lab.IndicatorDefinitions', N'IndicatorCode') IS NULL THROW 51000, 'Baseline column missing: lab.IndicatorDefinitions.IndicatorCode', 1;
    IF COL_LENGTH(N'lab.IndicatorDefinitions', N'IndicatorName') IS NULL THROW 51000, 'Baseline column missing: lab.IndicatorDefinitions.IndicatorName', 1;
    IF COL_LENGTH(N'lab.IndicatorDefinitions', N'Source') IS NULL THROW 51000, 'Baseline column missing: lab.IndicatorDefinitions.Source', 1;
    IF COL_LENGTH(N'lab.IndicatorDefinitions', N'Description') IS NULL THROW 51000, 'Baseline column missing: lab.IndicatorDefinitions.Description', 1;
    IF COL_LENGTH(N'lab.IndicatorDefinitions', N'DefaultWeight') IS NULL THROW 51000, 'Baseline column missing: lab.IndicatorDefinitions.DefaultWeight', 1;
    IF COL_LENGTH(N'lab.IndicatorDefinitions', N'Category') IS NULL THROW 51000, 'Baseline column missing: lab.IndicatorDefinitions.Category', 1;
    IF COL_LENGTH(N'lab.IndicatorDefinitions', N'IsActive') IS NULL THROW 51000, 'Baseline column missing: lab.IndicatorDefinitions.IsActive', 1;
    IF COL_LENGTH(N'lab.IndicatorDefinitions', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.IndicatorDefinitions.CreatedAt', 1;
    IF OBJECT_ID(N'lab.ScanIndicators', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.ScanIndicators', 1;
    IF COL_LENGTH(N'lab.ScanIndicators', N'ScanIndicatorId') IS NULL THROW 51000, 'Baseline column missing: lab.ScanIndicators.ScanIndicatorId', 1;
    IF COL_LENGTH(N'lab.ScanIndicators', N'ScanId') IS NULL THROW 51000, 'Baseline column missing: lab.ScanIndicators.ScanId', 1;
    IF COL_LENGTH(N'lab.ScanIndicators', N'IndicatorDefinitionId') IS NULL THROW 51000, 'Baseline column missing: lab.ScanIndicators.IndicatorDefinitionId', 1;
    IF COL_LENGTH(N'lab.ScanIndicators', N'WeightApplied') IS NULL THROW 51000, 'Baseline column missing: lab.ScanIndicators.WeightApplied', 1;
    IF COL_LENGTH(N'lab.ScanIndicators', N'ObservedValue') IS NULL THROW 51000, 'Baseline column missing: lab.ScanIndicators.ObservedValue', 1;
    IF COL_LENGTH(N'lab.ScanIndicators', N'Triggered') IS NULL THROW 51000, 'Baseline column missing: lab.ScanIndicators.Triggered', 1;
    IF COL_LENGTH(N'lab.ScanIndicators', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: lab.ScanIndicators.CreatedAt', 1;
    IF OBJECT_ID(N'benchmark.BenchmarkRuns', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: benchmark.BenchmarkRuns', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'BenchmarkId') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.BenchmarkId', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'ExperimentId') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.ExperimentId', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'RoundId') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.RoundId', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'WallabeeVersionId') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.WallabeeVersionId', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'HoneyBadgerVersionId') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.HoneyBadgerVersionId', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'TestHostId') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.TestHostId', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'BenchmarkType') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.BenchmarkType', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'StartedAt') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.StartedAt', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'CompletedAt') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.CompletedAt', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'DurationMs') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.DurationMs', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'SamplesScanned') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.SamplesScanned', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'SamplesDetected') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.SamplesDetected', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'SamplesMissed') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.SamplesMissed', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'TruePositives') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.TruePositives', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'TrueNegatives') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.TrueNegatives', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'FalsePositives') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.FalsePositives', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'FalseNegatives') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.FalseNegatives', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'DetectionRate') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.DetectionRate', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'EvasionRate') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.EvasionRate', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'PrecisionScore') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.PrecisionScore', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'RecallScore') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.RecallScore', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'F1Score') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.F1Score', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'AverageRiskScore') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.AverageRiskScore', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'AverageScanMs') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.AverageScanMs', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'P50ScanMs') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.P50ScanMs', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'P95ScanMs') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.P95ScanMs', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'P99ScanMs') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.P99ScanMs', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'FilesPerSecond') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.FilesPerSecond', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'MbPerSecond') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.MbPerSecond', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'CpuAveragePercent') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.CpuAveragePercent', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'CpuPeakPercent') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.CpuPeakPercent', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'MemoryAverageMb') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.MemoryAverageMb', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'MemoryPeakMb') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.MemoryPeakMb', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'ConfigurationJson') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.ConfigurationJson', 1;
    IF COL_LENGTH(N'benchmark.BenchmarkRuns', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: benchmark.BenchmarkRuns.CreatedAt', 1;
    IF OBJECT_ID(N'telemetry.SystemMetricSamples', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: telemetry.SystemMetricSamples', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'SystemMetricSampleId') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.SystemMetricSampleId', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'ExperimentId') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.ExperimentId', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'RoundId') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.RoundId', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'BenchmarkId') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.BenchmarkId', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'TestHostId') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.TestHostId', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'TimestampUtc') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.TimestampUtc', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'CpuPercent') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.CpuPercent', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'MemoryUsedMb') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.MemoryUsedMb', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'MemoryPercent') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.MemoryPercent', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'DiskReadBytesPerSecond') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.DiskReadBytesPerSecond', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'DiskWriteBytesPerSecond') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.DiskWriteBytesPerSecond', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'DiskReadOperationsPerSecond') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.DiskReadOperationsPerSecond', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'DiskWriteOperationsPerSecond') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.DiskWriteOperationsPerSecond', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'ProcessCpuPercent') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.ProcessCpuPercent', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'ProcessMemoryMb') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.ProcessMemoryMb', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'ThreadCount') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.ThreadCount', 1;
    IF COL_LENGTH(N'telemetry.SystemMetricSamples', N'HandleCount') IS NULL THROW 51000, 'Baseline column missing: telemetry.SystemMetricSamples.HandleCount', 1;
    IF OBJECT_ID(N'telemetry.TelemetryEvents', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: telemetry.TelemetryEvents', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'TelemetryEventId') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.TelemetryEventId', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'EventId') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.EventId', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'ExperimentId') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.ExperimentId', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'RoundId') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.RoundId', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'SampleId') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.SampleId', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'ScanId') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.ScanId', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'BenchmarkId') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.BenchmarkId', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'SourceComponent') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.SourceComponent', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'EventType') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.EventType', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'Severity') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.Severity', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'TimestampUtc') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.TimestampUtc', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'Message') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.Message', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'PayloadJson') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.PayloadJson', 1;
    IF COL_LENGTH(N'telemetry.TelemetryEvents', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: telemetry.TelemetryEvents.CreatedAt', 1;
    IF OBJECT_ID(N'lab.QuarantineEvents', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.QuarantineEvents', 1;
    IF COL_LENGTH(N'lab.QuarantineEvents', N'QuarantineEventId') IS NULL THROW 51000, 'Baseline column missing: lab.QuarantineEvents.QuarantineEventId', 1;
    IF COL_LENGTH(N'lab.QuarantineEvents', N'ScanId') IS NULL THROW 51000, 'Baseline column missing: lab.QuarantineEvents.ScanId', 1;
    IF COL_LENGTH(N'lab.QuarantineEvents', N'SampleId') IS NULL THROW 51000, 'Baseline column missing: lab.QuarantineEvents.SampleId', 1;
    IF COL_LENGTH(N'lab.QuarantineEvents', N'Sha256') IS NULL THROW 51000, 'Baseline column missing: lab.QuarantineEvents.Sha256', 1;
    IF COL_LENGTH(N'lab.QuarantineEvents', N'Action') IS NULL THROW 51000, 'Baseline column missing: lab.QuarantineEvents.Action', 1;
    IF COL_LENGTH(N'lab.QuarantineEvents', N'Reason') IS NULL THROW 51000, 'Baseline column missing: lab.QuarantineEvents.Reason', 1;
    IF COL_LENGTH(N'lab.QuarantineEvents', N'QuarantineReference') IS NULL THROW 51000, 'Baseline column missing: lab.QuarantineEvents.QuarantineReference', 1;
    IF COL_LENGTH(N'lab.QuarantineEvents', N'OccurredAt') IS NULL THROW 51000, 'Baseline column missing: lab.QuarantineEvents.OccurredAt', 1;
    IF OBJECT_ID(N'lab.ExperimentEvents', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: lab.ExperimentEvents', 1;
    IF COL_LENGTH(N'lab.ExperimentEvents', N'ExperimentEventId') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentEvents.ExperimentEventId', 1;
    IF COL_LENGTH(N'lab.ExperimentEvents', N'ExperimentId') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentEvents.ExperimentId', 1;
    IF COL_LENGTH(N'lab.ExperimentEvents', N'RoundId') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentEvents.RoundId', 1;
    IF COL_LENGTH(N'lab.ExperimentEvents', N'EventType') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentEvents.EventType', 1;
    IF COL_LENGTH(N'lab.ExperimentEvents', N'Message') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentEvents.Message', 1;
    IF COL_LENGTH(N'lab.ExperimentEvents', N'OccurredAt') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentEvents.OccurredAt', 1;
    IF COL_LENGTH(N'lab.ExperimentEvents', N'MetadataJson') IS NULL THROW 51000, 'Baseline column missing: lab.ExperimentEvents.MetadataJson', 1;
    IF OBJECT_ID(N'benchmark.DetectionConfusionMatrix', N'U') IS NULL THROW 51000, 'Incomplete DiveViewLab baseline: benchmark.DetectionConfusionMatrix', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'DetectionConfusionMatrixId') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.DetectionConfusionMatrixId', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'ExperimentId') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.ExperimentId', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'RoundId') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.RoundId', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'BenchmarkId') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.BenchmarkId', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'TruePositives') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.TruePositives', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'TrueNegatives') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.TrueNegatives', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'FalsePositives') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.FalsePositives', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'FalseNegatives') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.FalseNegatives', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'PrecisionScore') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.PrecisionScore', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'RecallScore') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.RecallScore', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'SpecificityScore') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.SpecificityScore', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'F1Score') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.F1Score', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'AccuracyScore') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.AccuracyScore', 1;
    IF COL_LENGTH(N'benchmark.DetectionConfusionMatrix', N'CreatedAt') IS NULL THROW 51000, 'Baseline column missing: benchmark.DetectionConfusionMatrix.CreatedAt', 1;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN
    ALTER TABLE lab.Scans ADD [HoneyBadgerVersionId] int NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Scans ADD [ExternalExperimentId] nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Scans ADD [ExternalRoundId] nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Scans ADD [ExternalSampleId] nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Scans ADD [ResponseAction] varchar(30) NOT NULL CONSTRAINT DF_Scans_ResponseAction DEFAULT 'None' WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Scans ADD [RetainSample] bit NOT NULL CONSTRAINT DF_Scans_RetainSample DEFAULT 0 WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Scans ADD [RetainReason] varchar(50) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Scans ADD [TelemetryPublished] bit NOT NULL CONSTRAINT DF_Scans_TelemetryPublished DEFAULT 0 WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.ScanIndicators ADD [IndicatorId] uniqueidentifier NOT NULL CONSTRAINT DF_ScanIndicators_IndicatorId DEFAULT NEWID() WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.ScanIndicators ADD [Source] nvarchar(100) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.ScanIndicators ADD [Name] nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.ScanIndicators ADD [Description] nvarchar(1000) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.ScanIndicators ADD [Category] varchar(50) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE benchmark.BenchmarkRuns ADD [LabeledSamples] int NOT NULL CONSTRAINT DF_BenchmarkRuns_LabeledSamples DEFAULT 0 WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE benchmark.BenchmarkRuns ADD [FalsePositiveRate] decimal(9,6) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE benchmark.BenchmarkRuns ADD [FalseNegativeRate] decimal(9,6) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE benchmark.BenchmarkRuns ADD [BytesPerSecond] decimal(18,4) NOT NULL CONSTRAINT DF_BenchmarkRuns_BytesPerSecond DEFAULT 0 WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE benchmark.BenchmarkRuns ADD [SamplesGenerated] int NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE benchmark.BenchmarkRuns ADD [ExternalExperimentId] nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE benchmark.BenchmarkRuns ADD [ExternalRoundId] nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE telemetry.SystemMetricSamples ADD [CpuPeakPercent] decimal(7,3) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE telemetry.SystemMetricSamples ADD [MemoryPeakMb] decimal(18,3) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE telemetry.TelemetryEvents ADD [State] varchar(20) NOT NULL CONSTRAINT DF_TelemetryEvents_State DEFAULT 'Published' WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE telemetry.TelemetryEvents ADD [Attempts] int NOT NULL CONSTRAINT DF_TelemetryEvents_Attempts DEFAULT 0 WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE telemetry.TelemetryEvents ADD [NextAttemptUtc] datetime2(3) NOT NULL CONSTRAINT DF_TelemetryEvents_NextAttemptUtc DEFAULT SYSUTCDATETIME() WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE telemetry.TelemetryEvents ADD [LastError] nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.QuarantineEvents ADD [OriginalPath] nvarchar(2048) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.QuarantineEvents ADD [QuarantinePath] nvarchar(2048) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.QuarantineEvents ADD [RestoredAt] datetime2(3) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.QuarantineEvents ADD [State] varchar(20) NOT NULL CONSTRAINT DF_QuarantineEvents_State DEFAULT 'Quarantined' WITH VALUES;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Experiments ALTER COLUMN ExternalExperimentId nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.ExperimentRounds ALTER COLUMN ExternalRoundId nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Samples ALTER COLUMN ExternalSampleId nvarchar(200) NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Samples ALTER COLUMN ExperimentId uniqueidentifier NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.QuarantineEvents ALTER COLUMN ScanId uniqueidentifier NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    IF EXISTS(SELECT 1 FROM telemetry.TelemetryEvents WHERE DATALENGTH(PayloadJson)>8000) THROW 51000, 'Historical telemetry payloads exceed 4000 characters; archive before upgrading.', 1;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE telemetry.TelemetryEvents DROP CONSTRAINT CK_TelemetryEvents_PayloadJson;
    ALTER TABLE telemetry.TelemetryEvents ALTER COLUMN PayloadJson nvarchar(4000) NULL;
    ALTER TABLE telemetry.TelemetryEvents ADD CONSTRAINT CK_TelemetryEvents_PayloadJson CHECK (PayloadJson IS NULL OR ISJSON(PayloadJson)=1);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE TABLE telemetry.TelemetryFailures (
     Id bigint IDENTITY PRIMARY KEY, EventId uniqueidentifier NOT NULL, TimestampUtc datetime2(3) NOT NULL,
     Error nvarchar(200) NOT NULL, Attempt int NOT NULL,
     CONSTRAINT FK_TelemetryFailures_Events FOREIGN KEY(EventId) REFERENCES telemetry.TelemetryEvents(EventId));

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    ALTER TABLE lab.Scans ADD CONSTRAINT FK_Scans_HoneyBadgerVersion FOREIGN KEY(HoneyBadgerVersionId) REFERENCES lab.HoneyBadgerVersions(HoneyBadgerVersionId);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE UNIQUE INDEX IX_Experiments_ExternalExperimentId ON lab.Experiments(ExternalExperimentId) WHERE ExternalExperimentId IS NOT NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE UNIQUE INDEX IX_ExperimentRounds_ExperimentId_ExternalRoundId ON lab.ExperimentRounds(ExperimentId,ExternalRoundId) WHERE ExternalRoundId IS NOT NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE UNIQUE INDEX IX_Samples_ExperimentId_RoundId_ExternalSampleId ON lab.Samples(ExperimentId,RoundId,ExternalSampleId) WHERE ExternalSampleId IS NOT NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE UNIQUE INDEX IX_ScanIndicators_IndicatorId ON lab.ScanIndicators(IndicatorId);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE INDEX IX_Scans_StartedAt ON lab.Scans(StartedAt);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE INDEX IX_Scans_DetectionLevel ON lab.Scans(DetectionLevel);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE INDEX IX_Scans_WasDetected ON lab.Scans(WasDetected);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE INDEX IX_Scans_RoundId ON lab.Scans(RoundId);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE INDEX IX_TelemetryEvents_State_NextAttemptUtc ON telemetry.TelemetryEvents(State,NextAttemptUtc);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    CREATE INDEX IX_TelemetryFailures_EventId ON telemetry.TelemetryFailures(EventId);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    UPDATE lab.QuarantineEvents SET State=Action;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    UPDATE s SET ExternalExperimentId=e.ExternalExperimentId, ExternalRoundId=r.ExternalRoundId, ExternalSampleId=p.ExternalSampleId
    FROM lab.Scans s LEFT JOIN lab.Experiments e ON e.ExperimentId=s.ExperimentId LEFT JOIN lab.ExperimentRounds r ON r.RoundId=s.RoundId LEFT JOIN lab.Samples p ON p.SampleId=s.SampleId;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN

    UPDATE b SET ExternalExperimentId=e.ExternalExperimentId, ExternalRoundId=r.ExternalRoundId
    FROM benchmark.BenchmarkRuns b LEFT JOIN lab.Experiments e ON e.ExperimentId=b.ExperimentId LEFT JOIN lab.ExperimentRounds r ON r.RoundId=b.RoundId;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930010707_InitialSqlServerMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930010707_InitialSqlServerMigration', N'10.0.12');
END;

COMMIT;
GO
BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930011426_PreserveVersionContractLengths'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[lab].[WallabeeVersions]') AND [c].[name] = N'Version');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [lab].[WallabeeVersions] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [lab].[WallabeeVersions] ALTER COLUMN [Version] varchar(100) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930011426_PreserveVersionContractLengths'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[lab].[HoneyBadgerVersions]') AND [c].[name] = N'Version');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [lab].[HoneyBadgerVersions] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [lab].[HoneyBadgerVersions] ALTER COLUMN [Version] varchar(100) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930011426_PreserveVersionContractLengths'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930011426_PreserveVersionContractLengths', N'10.0.12');
END;

COMMIT;
GO
BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930011839_AlignForeignKeyIndexes'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'benchmark.BenchmarkRuns') AND name=N'IX_BenchmarkRuns_HoneyBadgerVersionId') CREATE INDEX [IX_BenchmarkRuns_HoneyBadgerVersionId] ON [benchmark].[BenchmarkRuns] ([HoneyBadgerVersionId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'benchmark.BenchmarkRuns') AND name=N'IX_BenchmarkRuns_RoundId') CREATE INDEX [IX_BenchmarkRuns_RoundId] ON [benchmark].[BenchmarkRuns] ([RoundId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'benchmark.BenchmarkRuns') AND name=N'IX_BenchmarkRuns_TestHostId') CREATE INDEX [IX_BenchmarkRuns_TestHostId] ON [benchmark].[BenchmarkRuns] ([TestHostId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'benchmark.DetectionConfusionMatrix') AND name=N'IX_DetectionConfusionMatrix_BenchmarkId') CREATE INDEX [IX_DetectionConfusionMatrix_BenchmarkId] ON [benchmark].[DetectionConfusionMatrix] ([BenchmarkId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'benchmark.DetectionConfusionMatrix') AND name=N'IX_DetectionConfusionMatrix_ExperimentId') CREATE INDEX [IX_DetectionConfusionMatrix_ExperimentId] ON [benchmark].[DetectionConfusionMatrix] ([ExperimentId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'benchmark.DetectionConfusionMatrix') AND name=N'IX_DetectionConfusionMatrix_RoundId') CREATE INDEX [IX_DetectionConfusionMatrix_RoundId] ON [benchmark].[DetectionConfusionMatrix] ([RoundId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Experiments') AND name=N'IX_Experiments_DatasetId') CREATE INDEX [IX_Experiments_DatasetId] ON [lab].[Experiments] ([DatasetId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Experiments') AND name=N'IX_Experiments_ExternalExperimentId') CREATE UNIQUE INDEX [IX_Experiments_ExternalExperimentId] ON [lab].[Experiments] ([ExternalExperimentId]) WHERE [ExternalExperimentId] IS NOT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Experiments') AND name=N'IX_Experiments_HoneyBadgerVersionId') CREATE INDEX [IX_Experiments_HoneyBadgerVersionId] ON [lab].[Experiments] ([HoneyBadgerVersionId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Experiments') AND name=N'IX_Experiments_TestHostId') CREATE INDEX [IX_Experiments_TestHostId] ON [lab].[Experiments] ([TestHostId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.ExperimentEvents') AND name=N'IX_ExperimentEvents_RoundId') CREATE INDEX [IX_ExperimentEvents_RoundId] ON [lab].[ExperimentEvents] ([RoundId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.ExperimentRounds') AND name=N'IX_ExperimentRounds_ExperimentId_ExternalRoundId') CREATE UNIQUE INDEX [IX_ExperimentRounds_ExperimentId_ExternalRoundId] ON [lab].[ExperimentRounds] ([ExperimentId], [ExternalRoundId]) WHERE [ExternalRoundId] IS NOT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.QuarantineEvents') AND name=N'IX_QuarantineEvents_SampleId') CREATE INDEX [IX_QuarantineEvents_SampleId] ON [lab].[QuarantineEvents] ([SampleId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.QuarantineEvents') AND name=N'IX_QuarantineEvents_ScanId') CREATE INDEX [IX_QuarantineEvents_ScanId] ON [lab].[QuarantineEvents] ([ScanId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Samples') AND name=N'IX_Samples_ParentSampleId') CREATE INDEX [IX_Samples_ParentSampleId] ON [lab].[Samples] ([ParentSampleId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Samples') AND name=N'IX_Samples_RoundId') CREATE INDEX [IX_Samples_RoundId] ON [lab].[Samples] ([RoundId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Samples') AND name=N'IX_Samples_ExperimentId_RoundId_ExternalSampleId') CREATE UNIQUE INDEX [IX_Samples_ExperimentId_RoundId_ExternalSampleId] ON [lab].[Samples] ([ExperimentId], [RoundId], [ExternalSampleId]) WHERE [ExternalSampleId] IS NOT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Scans') AND name=N'IX_Scans_DetectionLevel') CREATE INDEX [IX_Scans_DetectionLevel] ON [lab].[Scans] ([DetectionLevel]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Scans') AND name=N'IX_Scans_HoneyBadgerVersionId') CREATE INDEX [IX_Scans_HoneyBadgerVersionId] ON [lab].[Scans] ([HoneyBadgerVersionId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Scans') AND name=N'IX_Scans_RoundId') CREATE INDEX [IX_Scans_RoundId] ON [lab].[Scans] ([RoundId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Scans') AND name=N'IX_Scans_StartedAt') CREATE INDEX [IX_Scans_StartedAt] ON [lab].[Scans] ([StartedAt]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Scans') AND name=N'IX_Scans_TestHostId') CREATE INDEX [IX_Scans_TestHostId] ON [lab].[Scans] ([TestHostId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.Scans') AND name=N'IX_Scans_WasDetected') CREATE INDEX [IX_Scans_WasDetected] ON [lab].[Scans] ([WasDetected]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'telemetry.SystemMetricSamples') AND name=N'IX_SystemMetricSamples_RoundId') CREATE INDEX [IX_SystemMetricSamples_RoundId] ON [telemetry].[SystemMetricSamples] ([RoundId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'telemetry.SystemMetricSamples') AND name=N'IX_SystemMetricSamples_TestHostId') CREATE INDEX [IX_SystemMetricSamples_TestHostId] ON [telemetry].[SystemMetricSamples] ([TestHostId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'telemetry.TelemetryEvents') AND name=N'IX_TelemetryEvents_BenchmarkId') CREATE INDEX [IX_TelemetryEvents_BenchmarkId] ON [telemetry].[TelemetryEvents] ([BenchmarkId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'telemetry.TelemetryEvents') AND name=N'IX_TelemetryEvents_RoundId') CREATE INDEX [IX_TelemetryEvents_RoundId] ON [telemetry].[TelemetryEvents] ([RoundId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'telemetry.TelemetryEvents') AND name=N'IX_TelemetryEvents_SampleId') CREATE INDEX [IX_TelemetryEvents_SampleId] ON [telemetry].[TelemetryEvents] ([SampleId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'telemetry.TelemetryEvents') AND name=N'IX_TelemetryEvents_ScanId') CREATE INDEX [IX_TelemetryEvents_ScanId] ON [telemetry].[TelemetryEvents] ([ScanId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'telemetry.TelemetryEvents') AND name=N'IX_TelemetryEvents_State_NextAttemptUtc') CREATE INDEX [IX_TelemetryEvents_State_NextAttemptUtc] ON [telemetry].[TelemetryEvents] ([State], [NextAttemptUtc]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'telemetry.TelemetryFailures') AND name=N'IX_TelemetryFailures_EventId') CREATE INDEX [IX_TelemetryFailures_EventId] ON [telemetry].[TelemetryFailures] ([EventId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'lab.ScanIndicators') AND name=N'IX_ScanIndicators_IndicatorId') CREATE UNIQUE INDEX [IX_ScanIndicators_IndicatorId] ON [lab].[ScanIndicators] ([IndicatorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930011839_AlignForeignKeyIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930011839_AlignForeignKeyIndexes', N'10.0.12');
END;

COMMIT;
GO
