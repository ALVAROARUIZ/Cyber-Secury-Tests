using Microsoft.EntityFrameworkCore;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;
using Wallabee.Infrastructure.Persistence.Mapping;
namespace Wallabee.Infrastructure.Persistence.Repositories;
public sealed class EfQuarantineRepository(IDbContextFactory<DiveViewDbContext> factory, EfUnitOfWork work) : IQuarantineRepository
{
    public Task SaveAsync(QuarantineEntry entry, CancellationToken ct = default) => work.WriteAsync(async (db, token) =>
    {
        var e = await db.QuarantineEvents.SingleOrDefaultAsync(x => x.QuarantineEventId == entry.Id, token);
        if (e is null) { e = new() { QuarantineEventId = entry.Id, OccurredAt = entry.QuarantinedAt.UtcDateTime, Sha256 = entry.Sha256 }; db.QuarantineEvents.Add(e); }
        e.OriginalPath = entry.OriginalPath; e.QuarantinePath = entry.QuarantinedPath; e.Reason = entry.Reason;
        e.RestoredAt = entry.RestoredAt?.UtcDateTime; e.State = entry.State;
        e.Action = entry.State is "Restored" or "Deleted" ? entry.State : "Quarantined";
    }, ct);
    public async Task<IReadOnlyList<QuarantineEntry>> ListAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return (await db.QuarantineEvents.AsNoTracking().Where(x => x.OriginalPath != null && x.QuarantinePath != null).OrderByDescending(x => x.OccurredAt).ToListAsync(ct))
            .Select(e => new QuarantineEntry(e.QuarantineEventId, e.OriginalPath!, e.QuarantinePath!, ScanEntityMapping.Utc(e.OccurredAt), e.Sha256) { Reason = e.Reason ?? "", State = e.State, RestoredAt = e.RestoredAt is {} date ? ScanEntityMapping.Utc(date) : null }).ToArray();
    }
}
