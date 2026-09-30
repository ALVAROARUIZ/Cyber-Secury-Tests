using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class ExperimentEventEntity
{
    public long ExperimentEventId { get; set; }
    public Guid ExperimentId { get; set; }
    public Guid? RoundId { get; set; }
    public string EventType { get; set; } = null!;
    public string? Message { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? MetadataJson { get; set; }
}
