using Microsoft.EntityFrameworkCore;
using Wallabee.Core.Interfaces;
namespace Wallabee.Infrastructure.Persistence.Repositories;
public sealed class EfSampleLabelRepository(EfUnitOfWork work) : ISampleLabelRepository
{
    public Task SetLabelsAsync(IReadOnlyDictionary<Guid, bool> scanLabels, CancellationToken ct = default) => work.WriteAsync(async (db, token) =>
    {
        foreach (var (scanId, positive) in scanLabels)
        {
            var sampleId = await db.Scans.Where(s => s.ScanId == scanId).Select(s => s.SampleId).SingleAsync(token);
            if (sampleId is {} id)
            {
                var sample = await db.Samples.SingleAsync(s => s.SampleId == id, token);
                var label = positive ? "Malicious" : "Benign";
                if (sample.GroundTruthLabel is not null and not "Unknown" && sample.GroundTruthLabel != label) throw new InvalidOperationException("Conflicting ground truth for existing sample.");
                sample.GroundTruthLabel = label;
            }
        }
    }, ct);
}
