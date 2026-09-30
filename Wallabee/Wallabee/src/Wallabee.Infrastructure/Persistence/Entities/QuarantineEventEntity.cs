using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class QuarantineEventEntity
{
    public Guid QuarantineEventId { get; set; }
    public Guid? ScanId { get; set; }
    public Guid? SampleId { get; set; }
    public string Sha256 { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string? Reason { get; set; }
    public string? QuarantineReference { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? OriginalPath { get; set; }
    public string? QuarantinePath { get; set; }
    public DateTime? RestoredAt { get; set; }
    public string State { get; set; } = null!;
}
