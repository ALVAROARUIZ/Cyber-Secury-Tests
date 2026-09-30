using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class TelemetryFailureEntity
{
    public long Id { get; set; }
    public Guid EventId { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string Error { get; set; } = null!;
    public int Attempt { get; set; }
}
