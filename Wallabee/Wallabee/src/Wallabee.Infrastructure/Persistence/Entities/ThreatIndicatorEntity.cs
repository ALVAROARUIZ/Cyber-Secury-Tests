using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class ThreatIndicatorEntity
{
    public long ScanIndicatorId { get; set; }
    public Guid ScanId { get; set; }
    public int IndicatorDefinitionId { get; set; }
    public decimal? WeightApplied { get; set; }
    public string? ObservedValue { get; set; }
    public bool Triggered { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid IndicatorId { get; set; }
    public string? Source { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public ScanEntity Scan { get; set; } = null!;
    public IndicatorDefinitionEntity Definition { get; set; } = null!;
}
