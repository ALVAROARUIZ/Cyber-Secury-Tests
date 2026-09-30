using Wallabee.Core.Enums;
using Wallabee.Core.Models;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Mapping;
public static class ScanEntityMapping
{
    public static FileScanResult ToDomain(this ScanEntity e) => new(e.FileName ?? "", e.FileSizeBytes, e.Sha256, (double)e.RiskScore, e.DetectionLevel,
        e.Indicators.Select(i => new ThreatIndicator(i.Source ?? i.Definition.Source, i.Name ?? i.Definition.IndicatorName, i.Description ?? i.Definition.Description ?? "", (double)(i.WeightApplied ?? 0), Enum.TryParse<ThreatCategory>(i.Category, out var c) ? c : ThreatCategory.Unknown) { IndicatorId = i.IndicatorId, ScanId = e.ScanId }).ToArray(),
        TimeSpan.FromMilliseconds((double)e.DurationMs), e.ErrorMessage)
    { ScanId = e.ScanId, StartedAt = Utc(e.StartedAt), CompletedAt = Utc(e.CompletedAt), WallabeeVersion = e.WallabeeVersion.Version,
        HoneyBadgerVersion = e.HoneyBadgerVersion?.Version, ExperimentId = e.ExternalExperimentId, RoundId = e.ExternalRoundId, SampleId = e.ExternalSampleId,
        RetainSample = e.RetainSample, RetainReason = e.RetainReason, ResponseAction = Enum.TryParse<ResponseAction>(e.ResponseAction, out var action) ? action : ResponseAction.None };
    public static DateTimeOffset Utc(DateTime date) => new(DateTime.SpecifyKind(date, DateTimeKind.Utc));
}
