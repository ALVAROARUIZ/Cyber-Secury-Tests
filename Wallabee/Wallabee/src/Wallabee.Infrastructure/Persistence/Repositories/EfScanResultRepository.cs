using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wallabee.Core.Configuration;
using Wallabee.Core.Enums;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;
using Wallabee.Infrastructure.Persistence.Entities;
using Wallabee.Infrastructure.Persistence.Mapping;
namespace Wallabee.Infrastructure.Persistence.Repositories;
public sealed class EfScanResultRepository(IDbContextFactory<DiveViewDbContext> factory, EfUnitOfWork work, IOptions<PersistenceOptions> options) : IScanResultRepository
{
    public Task SaveAsync(FileScanResult result, TelemetryEvent telemetry, CancellationToken ct = default) => work.WriteAsync(async (db, token) =>
    {
        // Retry-safe after an ambiguous commit: a scan ID is immutable.
        if (await db.Scans.AnyAsync(s => s.ScanId == result.ScanId, token)) return;
        var refs = await new ReferenceResolver(db).ResolveAsync(result.WallabeeVersion, result.HoneyBadgerVersion, result.ExperimentId, result.RoundId, options.Value.TestHostAlias, token);
        var sample = result.SampleId is null ? null : await db.Samples.SingleOrDefaultAsync(s => s.ExperimentId == refs.Experiment && s.RoundId == refs.Round && s.ExternalSampleId == result.SampleId, token);
        if (sample is null)
        {
            sample = new SampleEntity { SampleId = Guid.NewGuid(), ExternalSampleId = result.SampleId, ExperimentId = refs.Experiment, RoundId = refs.Round,
                FileName = result.FileName, Sha256 = result.Sha256 ?? "", SizeBytes = result.FileSize, RetainSample = result.RetainSample, RetainReason = result.RetainReason };
            db.Samples.Add(sample); await db.SaveChangesAsync(token);
        }
        else if (sample.Sha256 != result.Sha256 || sample.SizeBytes != result.FileSize) throw new InvalidOperationException("SampleId already identifies different sample metadata.");
        var scan = new ScanEntity { ScanId = result.ScanId, SampleId = sample.SampleId, ExperimentId = refs.Experiment, RoundId = refs.Round,
            WallabeeVersionId = refs.Wallabee, HoneyBadgerVersionId = refs.HoneyBadger, TestHostId = refs.Host,
            FileName = result.FileName, Sha256 = result.Sha256 ?? "", FileSizeBytes = result.FileSize, RiskScore = (decimal)result.RiskScore,
            DetectionLevel = result.DetectionLevel, WasDetected = result.DetectionLevel >= DetectionLevel.Medium,
            StartedAt = result.StartedAt.UtcDateTime, CompletedAt = result.CompletedAt.UtcDateTime, DurationMs = (decimal)result.DurationMs,
            ErrorOccurred = result.Error is not null, ErrorMessage = result.Error,
            ExternalExperimentId = result.ExperimentId, ExternalRoundId = result.RoundId, ExternalSampleId = result.SampleId,
            ResponseAction = result.ResponseAction.ToString(), RetainSample = result.RetainSample, RetainReason = result.RetainReason };
        db.Scans.Add(scan);
        foreach (var i in result.Indicators)
        {
            var code = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(i.Source + ":" + i.Name)));
            var definition = await db.IndicatorDefinitions.SingleOrDefaultAsync(x => x.IndicatorCode == code, token);
            if (definition is null) { definition = new() { IndicatorCode = code, IndicatorName = i.Name, Source = i.Source, Description = i.Description, DefaultWeight = (decimal)i.Weight, IsActive = true }; db.IndicatorDefinitions.Add(definition); await db.SaveChangesAsync(token); }
            scan.Indicators.Add(new() { ScanId = scan.ScanId, IndicatorId = i.IndicatorId, IndicatorDefinitionId = definition.IndicatorDefinitionId,
                Source = i.Source, Name = i.Name, Description = i.Description, Category = i.Category.ToString(), WeightApplied = (decimal)i.Weight, Triggered = true });
        }
        var item = EfTelemetryQueue.CreateEvent(telemetry);
        item.ScanId = scan.ScanId; item.SampleId = sample.SampleId; item.ExperimentId = refs.Experiment; item.RoundId = refs.Round;
        item.PayloadJson = null; db.TelemetryEvents.Add(item);
    }, ct);
    internal static IQueryable<ScanEntity> Query(DiveViewDbContext db) => db.Scans.AsNoTracking().Include(s => s.WallabeeVersion).Include(s => s.HoneyBadgerVersion).Include(s => s.Indicators).ThenInclude(i => i.Definition);
    public async Task<FileScanResult?> GetAsync(Guid id, CancellationToken ct = default)
    { await using var db = await factory.CreateDbContextAsync(ct); return (await Query(db).SingleOrDefaultAsync(x => x.ScanId == id, ct))?.ToDomain(); }
    public async Task<IReadOnlyList<FileScanResult>> FindAsync(ScanFilter f, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct); var query = Query(db);
        if (f.Sha256 is not null) query = query.Where(s => s.Sha256 == f.Sha256.ToUpper());
        if (f.ExperimentId is not null) query = query.Where(s => s.ExternalExperimentId == f.ExperimentId);
        if (f.RoundId is not null) query = query.Where(s => s.ExternalRoundId == f.RoundId);
        if (f.SampleId is not null) query = query.Where(s => s.ExternalSampleId == f.SampleId);
        return (await query.OrderByDescending(s => s.StartedAt).ThenBy(s => s.ScanId).Skip(Math.Max(0, f.Offset)).Take(Math.Clamp(f.Limit, 1, 1000)).ToListAsync(ct)).Select(s => s.ToDomain()).ToArray();
    }
}
