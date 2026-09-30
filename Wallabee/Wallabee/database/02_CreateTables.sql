USE [DiveViewLab];
GO

CREATE TABLE lab.WallabeeVersions (
    WallabeeVersionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WallabeeVersions PRIMARY KEY,
    Version VARCHAR(50) NOT NULL, MajorVersion INT NULL, MinorVersion INT NULL, PatchVersion INT NULL,
    BuildMetadata VARCHAR(100) NULL, CommitHash VARCHAR(100) NULL, BranchName VARCHAR(100) NULL,
    BuildDate DATETIME2(3) NULL, Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_WallabeeVersions_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE lab.HoneyBadgerVersions (
    HoneyBadgerVersionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoneyBadgerVersions PRIMARY KEY,
    Version VARCHAR(50) NOT NULL, MajorVersion INT NULL, MinorVersion INT NULL, PatchVersion INT NULL,
    CommitHash VARCHAR(100) NULL, BranchName VARCHAR(100) NULL, BuildDate DATETIME2(3) NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_HoneyBadgerVersions_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

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
GO

CREATE TABLE lab.Datasets (
    DatasetId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Datasets PRIMARY KEY CONSTRAINT DF_Datasets_Id DEFAULT NEWSEQUENTIALID(),
    DatasetName NVARCHAR(200) NOT NULL, DatasetVersion VARCHAR(50) NULL, Description NVARCHAR(1000) NULL,
    Seed BIGINT NULL, TotalSamples INT NULL, BenignSamples INT NULL, SyntheticSamples INT NULL,
    ManifestSha256 CHAR(64) NULL,
    CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Datasets_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE lab.Experiments (
    ExperimentId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Experiments PRIMARY KEY CONSTRAINT DF_Experiments_Id DEFAULT NEWSEQUENTIALID(),
    ExternalExperimentId VARCHAR(100) NULL, ExperimentName NVARCHAR(200) NOT NULL, Description NVARCHAR(1000) NULL,
    WallabeeVersionId INT NOT NULL, HoneyBadgerVersionId INT NULL, DatasetId UNIQUEIDENTIFIER NULL, TestHostId INT NULL,
    ExperimentType VARCHAR(50) NULL, Status VARCHAR(30) NOT NULL, StartedAt DATETIME2(3) NOT NULL,
    CompletedAt DATETIME2(3) NULL, RandomSeed BIGINT NULL, TotalRounds INT NULL, TotalSamples INT NULL,
    ConfigurationJson NVARCHAR(MAX) NULL, IsArchived BIT NOT NULL CONSTRAINT DF_Experiments_IsArchived DEFAULT 0,
    ArchivedAt DATETIME2(3) NULL, CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Experiments_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

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
GO

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
GO

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
GO

CREATE TABLE lab.IndicatorDefinitions (
    IndicatorDefinitionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndicatorDefinitions PRIMARY KEY,
    IndicatorCode VARCHAR(100) NOT NULL, IndicatorName NVARCHAR(200) NOT NULL, Source NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000) NULL, DefaultWeight DECIMAL(9,4) NULL, Category NVARCHAR(100) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_IndicatorDefinitions_IsActive DEFAULT 1,
    CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_IndicatorDefinitions_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE lab.ScanIndicators (
    ScanIndicatorId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScanIndicators PRIMARY KEY,
    ScanId UNIQUEIDENTIFIER NOT NULL, IndicatorDefinitionId INT NOT NULL, WeightApplied DECIMAL(9,4) NULL,
    ObservedValue NVARCHAR(500) NULL, Triggered BIT NOT NULL CONSTRAINT DF_ScanIndicators_Triggered DEFAULT 1,
    CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_ScanIndicators_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

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
GO

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
GO

CREATE TABLE telemetry.TelemetryEvents (
    TelemetryEventId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TelemetryEvents PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL, ExperimentId UNIQUEIDENTIFIER NULL, RoundId UNIQUEIDENTIFIER NULL,
    SampleId UNIQUEIDENTIFIER NULL, ScanId UNIQUEIDENTIFIER NULL, BenchmarkId UNIQUEIDENTIFIER NULL,
    SourceComponent VARCHAR(100) NOT NULL, EventType VARCHAR(150) NOT NULL, Severity VARCHAR(30) NULL,
    TimestampUtc DATETIME2(3) NOT NULL, Message NVARCHAR(1000) NULL, PayloadJson NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_TelemetryEvents_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE lab.QuarantineEvents (
    QuarantineEventId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_QuarantineEvents PRIMARY KEY CONSTRAINT DF_QuarantineEvents_Id DEFAULT NEWSEQUENTIALID(),
    ScanId UNIQUEIDENTIFIER NOT NULL, SampleId UNIQUEIDENTIFIER NULL, Sha256 CHAR(64) NOT NULL,
    Action VARCHAR(30) NOT NULL, Reason NVARCHAR(500) NULL, QuarantineReference NVARCHAR(500) NULL,
    OccurredAt DATETIME2(3) NOT NULL
);
GO

CREATE TABLE lab.ExperimentEvents (
    ExperimentEventId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExperimentEvents PRIMARY KEY,
    ExperimentId UNIQUEIDENTIFIER NOT NULL, RoundId UNIQUEIDENTIFIER NULL, EventType VARCHAR(100) NOT NULL,
    Message NVARCHAR(1000) NULL, OccurredAt DATETIME2(3) NOT NULL, MetadataJson NVARCHAR(MAX) NULL
);
GO

CREATE TABLE benchmark.DetectionConfusionMatrix (
    DetectionConfusionMatrixId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DetectionConfusionMatrix PRIMARY KEY,
    ExperimentId UNIQUEIDENTIFIER NOT NULL, RoundId UNIQUEIDENTIFIER NULL, BenchmarkId UNIQUEIDENTIFIER NULL,
    TruePositives INT NOT NULL, TrueNegatives INT NOT NULL, FalsePositives INT NOT NULL, FalseNegatives INT NOT NULL,
    PrecisionScore DECIMAL(9,6) NULL, RecallScore DECIMAL(9,6) NULL, SpecificityScore DECIMAL(9,6) NULL,
    F1Score DECIMAL(9,6) NULL, AccuracyScore DECIMAL(9,6) NULL,
    CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_DetectionConfusionMatrix_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO