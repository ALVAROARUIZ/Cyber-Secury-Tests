namespace Wallabee.Contracts.Telemetry;
public sealed record IndicatorDto(Guid IndicatorId, Guid ScanId, string Source, string Name, string Description, double Weight);
public sealed record ScanTelemetry(Guid ScanId, string FileName, string? Sha256, long FileSize, double RiskScore, string DetectionLevel,
    double ScanDurationMs, string WallabeeVersion, string? ExperimentId, string? RoundId, string? SampleId, string? HoneyBadgerVersion,
    DateTimeOffset Timestamp, IReadOnlyList<IndicatorDto> Indicators, bool RetainSample, string? RetainReason);
public sealed record TelemetryEnvelope(int SchemaVersion, Guid EventId, string EventType, System.Text.Json.JsonElement Data);
public sealed record TelemetryAck(Guid EventId, bool Accepted);
public sealed record SystemMetricsDto(double CpuAverage, double CpuPeak, double RamAverageMb, double RamPeakMb, DateTimeOffset Timestamp);
