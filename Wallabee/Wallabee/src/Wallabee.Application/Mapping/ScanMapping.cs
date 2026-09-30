using System.Text.Json;
using System.Text.Json.Serialization;
using Wallabee.Contracts.Requests;
using Wallabee.Contracts.Responses;
using Wallabee.Contracts.Telemetry;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;
namespace Wallabee.Application.Mapping;
public static class ScanMapping
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    public static ExperimentContext ToContext(this ExperimentRequest r) => new(r.ExperimentId, r.RoundId, r.SampleId, r.HoneyBadgerVersion, r.RetainSample, r.RetainReason);
    public static IndicatorDto ToDto(this ThreatIndicator i) => new(i.IndicatorId, i.ScanId, i.Source, i.Name, i.Description, i.Weight);
    public static ScanResponse ToResponse(this FileScanResult r) => new(r.ScanId, r.FileName, r.Sha256, r.FileSize, r.RiskScore, r.DetectionLevel.ToString(), r.StartedAt, r.CompletedAt, r.DurationMs, r.WallabeeVersion, r.ExperimentId, r.RoundId, r.SampleId, r.HoneyBadgerVersion, r.Indicators.Select(ToDto).ToArray(), r.ResponseAction.ToString(), r.RetainSample, r.RetainReason);
    public static TelemetryEvent ToTelemetry(this FileScanResult r) => new(r.ScanId, "scan", JsonSerializer.Serialize(new ScanTelemetry(r.ScanId, r.FileName, r.Sha256, r.FileSize, r.RiskScore, r.DetectionLevel.ToString(), r.DurationMs, r.WallabeeVersion, r.ExperimentId, r.RoundId, r.SampleId, r.HoneyBadgerVersion, r.CompletedAt, r.Indicators.Select(ToDto).ToArray(), r.RetainSample, r.RetainReason), Json), r.ScanId);
}
