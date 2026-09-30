using Microsoft.EntityFrameworkCore;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Mapping;
internal sealed class ReferenceResolver(DiveViewDbContext db)
{
    public async Task<(int Wallabee, int? HoneyBadger, int Host, Guid? Experiment, Guid? Round)> ResolveAsync(string version, string? honeyBadger, string? experiment, string? round, string hostAlias, CancellationToken ct)
    {
        var w = await db.WallabeeVersions.SingleOrDefaultAsync(x => x.Version == version, ct);
        if (w is null) { w = new() { Version = version }; db.WallabeeVersions.Add(w); }
        HoneyBadgerVersionEntity? h = null;
        if (honeyBadger is not null) { h = await db.HoneyBadgerVersions.SingleOrDefaultAsync(x => x.Version == honeyBadger, ct); if (h is null) { h = new() { Version = honeyBadger }; db.HoneyBadgerVersions.Add(h); } }
        var host = await db.TestHosts.FirstOrDefaultAsync(x => x.HostName == hostAlias, ct);
        if (host is null) { host = new() { HostName = hostAlias, HostAlias = hostAlias, IsActive = true }; db.TestHosts.Add(host); }
        await db.SaveChangesAsync(ct);
        Guid? experimentKey = null; Guid? roundKey = null;
        if (experiment is not null)
        {
            var e = await db.Experiments.SingleOrDefaultAsync(x => x.ExternalExperimentId == experiment, ct);
            if (e is null) { e = new() { ExperimentId = Guid.NewGuid(), ExternalExperimentId = experiment, ExperimentName = experiment, WallabeeVersionId = w.WallabeeVersionId, HoneyBadgerVersionId = h?.HoneyBadgerVersionId, TestHostId = host.TestHostId, Status = "Running", StartedAt = DateTime.UtcNow }; db.Experiments.Add(e); await db.SaveChangesAsync(ct); }
            experimentKey = e.ExperimentId;
            if (round is not null)
            {
                var r = await db.ExperimentRounds.SingleOrDefaultAsync(x => x.ExperimentId == e.ExperimentId && x.ExternalRoundId == round, ct);
                if (r is null)
                {
                    var number = (await db.ExperimentRounds.Where(x => x.ExperimentId == e.ExperimentId).MaxAsync(x => (int?)x.RoundNumber, ct) ?? 0) + 1;
                    r = new() { RoundId = Guid.NewGuid(), ExperimentId = e.ExperimentId, ExternalRoundId = round, RoundNumber = number, StartedAt = DateTime.UtcNow };
                    db.ExperimentRounds.Add(r); await db.SaveChangesAsync(ct);
                }
                roundKey = r.RoundId;
            }
        }
        return (w.WallabeeVersionId, h?.HoneyBadgerVersionId, host.TestHostId, experimentKey, roundKey);
    }
}
