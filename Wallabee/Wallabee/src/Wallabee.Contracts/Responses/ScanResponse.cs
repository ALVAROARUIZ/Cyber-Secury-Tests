using Wallabee.Contracts.Telemetry;
namespace Wallabee.Contracts.Responses;
public sealed record ScanResponse(Guid ScanId, string FileName, string? Sha256, long FileSize, double RiskScore, string DetectionLevel,
    DateTimeOffset StartedAt, DateTimeOffset CompletedAt, double DurationMs, string WallabeeVersion,
    string? ExperimentId, string? RoundId, string? SampleId, string? HoneyBadgerVersion,
    IReadOnlyList<IndicatorDto> Indicators, string ResponseAction, bool RetainSample, string? RetainReason);
