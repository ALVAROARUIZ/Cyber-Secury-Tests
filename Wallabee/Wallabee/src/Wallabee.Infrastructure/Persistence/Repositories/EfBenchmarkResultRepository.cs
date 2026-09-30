using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wallabee.Application.Mapping;
using Wallabee.Contracts.Benchmark;
using Wallabee.Contracts.Experiments;
using Wallabee.Contracts.Telemetry;
using Wallabee.Core.Configuration;
using Wallabee.Core.Interfaces;
using Wallabee.Infrastructure.Persistence.Mapping;
namespace Wallabee.Infrastructure.Persistence.Repositories;
// Preserves the application port; JSON is decoded at the boundary, never stored as a metrics document.
public sealed class EfBenchmarkResultRepository(IDbContextFactory<DiveViewDbContext> factory, EfUnitOfWork work, IOptions<PersistenceOptions> options) : IResultDocumentRepository
{
    public Task SaveAsync(Guid id, string kind, string json, IReadOnlyList<TelemetryEvent> events, CancellationToken ct = default) => work.WriteAsync(async (db, token) =>
    {
        if (kind != "benchmark") throw new ArgumentException("Unsupported result kind.");
        if (await db.BenchmarkRuns.AnyAsync(x => x.BenchmarkId == id, token)) return;
        var dto = JsonSerializer.Deserialize<BenchmarkDto>(json, ScanMapping.Json) ?? throw new ArgumentException("Invalid benchmark.");
        if (dto.BenchmarkId != id) throw new ArgumentException("Benchmark ID mismatch.");
        var refs = await new ReferenceResolver(db).ResolveAsync(dto.WallabeeVersion, dto.HoneyBadgerVersion, dto.ExperimentId, dto.RoundId, options.Value.TestHostAlias, token);
        var entity = BenchmarkEntityMapping.ToEntity(dto); entity.WallabeeVersionId = refs.Wallabee; entity.HoneyBadgerVersionId = refs.HoneyBadger;
        entity.ExperimentId = refs.Experiment; entity.RoundId = refs.Round; entity.TestHostId = refs.Host;
        var round = events.FirstOrDefault(e => e.EventType == "round");
        if (round is not null) entity.SamplesGenerated = JsonSerializer.Deserialize<RoundSummary>(round.Payload, ScanMapping.Json)!.SamplesGenerated;
        db.BenchmarkRuns.Add(entity); await db.SaveChangesAsync(token);
        if (refs.Round is {} roundId)
        {
            var aggregate = await db.ExperimentRounds.SingleAsync(r => r.RoundId == roundId, token);
            if (aggregate.CompletedAt is null || aggregate.CompletedAt <= entity.CompletedAt)
            {
                aggregate.CompletedAt = entity.CompletedAt;
                aggregate.SamplesGenerated = entity.SamplesGenerated ?? entity.SamplesScanned;
                aggregate.SamplesScanned = entity.SamplesScanned; aggregate.SamplesDetected = entity.SamplesDetected ?? 0; aggregate.SamplesMissed = entity.SamplesMissed ?? 0;
                aggregate.TruePositives = entity.TruePositives; aggregate.TrueNegatives = entity.TrueNegatives; aggregate.FalsePositives = entity.FalsePositives; aggregate.FalseNegatives = entity.FalseNegatives;
                aggregate.DetectionRate = entity.DetectionRate; aggregate.EvasionRate = entity.EvasionRate; aggregate.PrecisionScore = entity.PrecisionScore; aggregate.RecallScore = entity.RecallScore; aggregate.F1Score = entity.F1Score;
                aggregate.AverageScanMs = entity.AverageScanMs; aggregate.P50ScanMs = entity.P50ScanMs; aggregate.P95ScanMs = entity.P95ScanMs; aggregate.P99ScanMs = entity.P99ScanMs;
                aggregate.FilesPerSecond = entity.FilesPerSecond; aggregate.MbPerSecond = entity.MbPerSecond; aggregate.CpuAveragePercent = entity.CpuAveragePercent; aggregate.CpuPeakPercent = entity.CpuPeakPercent;
                aggregate.MemoryAverageMb = entity.MemoryAverageMb; aggregate.MemoryPeakMb = entity.MemoryPeakMb;
            }
        }
        if (refs.Experiment is {} experimentId)
            db.DetectionConfusionMatrix.Add(new() { DetectionConfusionMatrixId = 0, ExperimentId = experimentId, RoundId = refs.Round, BenchmarkId = id,
                TruePositives = dto.TruePositives, TrueNegatives = dto.TrueNegatives, FalsePositives = dto.FalsePositives, FalseNegatives = dto.FalseNegatives,
                PrecisionScore = (decimal?)dto.Precision, RecallScore = (decimal?)dto.Recall, F1Score = (decimal?)dto.F1Score });
        foreach (var item in events)
        {
            var record = EfTelemetryQueue.CreateEvent(item); record.BenchmarkId = id; record.ExperimentId = refs.Experiment; record.RoundId = refs.Round;
            if (item.EventType is "benchmark" or "round") record.PayloadJson = null;
            if (item.EventType == "system")
            {
                var m = JsonSerializer.Deserialize<SystemMetricsDto>(item.Payload, ScanMapping.Json)!;
                db.SystemMetricSamples.Add(new() { BenchmarkId = id, ExperimentId = refs.Experiment, RoundId = refs.Round, TestHostId = refs.Host,
                    TimestampUtc = m.Timestamp.UtcDateTime, CpuPercent = (decimal)m.CpuAverage, CpuPeakPercent = (decimal)m.CpuPeak,
                    MemoryUsedMb = (decimal)m.RamAverageMb, MemoryPeakMb = (decimal)m.RamPeakMb });
            }
            db.TelemetryEvents.Add(record);
        }
    }, ct);
    internal static IQueryable<Entities.BenchmarkResultEntity> Query(DiveViewDbContext db) => db.BenchmarkRuns.AsNoTracking().Include(x => x.WallabeeVersion).Include(x => x.HoneyBadgerVersion);
    public async Task<string?> GetAsync(Guid? id, string kind, CancellationToken ct = default)
    {
        if (kind != "benchmark") return null;
        await using var db = await factory.CreateDbContextAsync(ct);
        var result = await Query(db).Where(x => id == null || x.BenchmarkId == id).OrderByDescending(x => x.StartedAt).FirstOrDefaultAsync(ct);
        return result is null ? null : JsonSerializer.Serialize(result.ToDto(), ScanMapping.Json);
    }
}
