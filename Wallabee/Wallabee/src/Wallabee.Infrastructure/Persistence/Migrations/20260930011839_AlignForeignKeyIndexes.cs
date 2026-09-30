using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallabee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignForeignKeyIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
                """);
        }
        protected override void Down(MigrationBuilder migrationBuilder) =>
            throw new NotSupportedException("Indexes may predate EF adoption; automatic removal is disabled.");
    }
}
