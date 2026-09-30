using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class IndicatorDefinitionEntity
{
    public int IndicatorDefinitionId { get; set; }
    public string IndicatorCode { get; set; } = null!;
    public string IndicatorName { get; set; } = null!;
    public string Source { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? DefaultWeight { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
