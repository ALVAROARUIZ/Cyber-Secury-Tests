using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class DatasetEntity
{
    public Guid DatasetId { get; set; }
    public string DatasetName { get; set; } = null!;
    public string? DatasetVersion { get; set; }
    public string? Description { get; set; }
    public long? Seed { get; set; }
    public int? TotalSamples { get; set; }
    public int? BenignSamples { get; set; }
    public int? SyntheticSamples { get; set; }
    public string? ManifestSha256 { get; set; }
    public DateTime CreatedAt { get; set; }
}
