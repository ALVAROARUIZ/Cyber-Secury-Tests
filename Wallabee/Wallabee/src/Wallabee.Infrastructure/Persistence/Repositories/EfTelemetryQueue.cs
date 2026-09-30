using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wallabee.Application.Mapping;
using Wallabee.Contracts.Experiments;
using Wallabee.Core.Interfaces;
using Wallabee.Infrastructure.Persistence.Entities;
using Wallabee.Infrastructure.Persistence.Mapping;
namespace Wallabee.Infrastructure.Persistence.Repositories;
public sealed class EfTelemetryQueue(IDbContextFactory<DiveViewDbContext> factory, EfUnitOfWork work) : ITelemetryQueue
{
    internal static TelemetryEventEntity CreateEvent(TelemetryEvent item)
    {
        // Scan/benchmark payloads are reconstructed from relational facts.
        var payload = item.EventType is "scan" or "benchmark" or "round" ? null : item.Payload;
        if (payload?.Length > 4000) throw new ArgumentException("Telemetry payload exceeds 4000 characters.");
        if (payload is not null) { using var parsed = JsonDocument.Parse(payload); }
        return new() { EventId = item.EventId, EventType = item.EventType, SourceComponent = "Wallabee", Severity = "Information",
            TimestampUtc = DateTime.UtcNow, PayloadJson = payload, ScanId = item.ScanId, State = "Pending", NextAttemptUtc = DateTime.UtcNow };
    }
    public Task EnqueueAsync(TelemetryEvent item, CancellationToken ct = default) => work.WriteAsync(async (db, token) =>
    {
        if (await db.TelemetryEvents.AnyAsync(x => x.EventId == item.EventId, token)) return;
        var entity = CreateEvent(item);
        if (item.EventType is "scan" or "benchmark" or "round")
        {
            // Standalone enqueue may carry a small metadata DTO; transactional repositories set relational references.
            if (item.Payload.Length > 4000) throw new ArgumentException("Telemetry payload exceeds 4000 characters.");
            using var document = JsonDocument.Parse(item.Payload); entity.PayloadJson = item.Payload;
        }
        db.TelemetryEvents.Add(entity);
    }, ct);
    public async Task<IReadOnlyList<TelemetryEvent>> GetDueAsync(int limit, DateTimeOffset now, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var timestamp = now.UtcDateTime;
        var rows = await db.TelemetryEvents.AsNoTracking().Where(x => x.State == "Pending" && x.NextAttemptUtc <= timestamp).OrderBy(x => x.NextAttemptUtc).Take(Math.Clamp(limit, 1, 1000)).ToListAsync(ct);
        var results = new List<TelemetryEvent>();
        foreach (var row in rows)
        {
            var json = row.PayloadJson;
            if (json is null && row.ScanId is {} scanId)
            {
                var scan = await EfScanResultRepository.Query(db).SingleAsync(x => x.ScanId == scanId, ct);
                json = scan.ToDomain().ToTelemetry().Payload;
            }
            if (json is null && row.BenchmarkId is {} benchmarkId)
            {
                var benchmark = await EfBenchmarkResultRepository.Query(db).SingleAsync(x => x.BenchmarkId == benchmarkId, ct);
                json = JsonSerializer.Serialize(benchmark.ToDto(), ScanMapping.Json);
                if (row.EventType == "round") json = JsonSerializer.Serialize(JsonSerializer.Deserialize<RoundSummary>(json, ScanMapping.Json)! with { SamplesGenerated = benchmark.SamplesGenerated ?? benchmark.SamplesScanned }, ScanMapping.Json);
            }
            if (json is null) throw new InvalidOperationException("Telemetry event has no relational result or payload.");
            results.Add(new(row.EventId, row.EventType, json, row.ScanId, row.Attempts));
        }
        return results;
    }
    public Task AcknowledgeAsync(Guid id, CancellationToken ct = default) => work.WriteAsync(async (db, token) =>
    {
        var item = await db.TelemetryEvents.SingleAsync(x => x.EventId == id, token); item.State = "Published";
        if (item.ScanId is {} scanId) { var scan = await db.Scans.SingleAsync(x => x.ScanId == scanId, token); scan.TelemetryPublished = true; }
    }, ct);
    public Task FailAsync(Guid id, string error, DateTimeOffset nextAttempt, bool exhausted, CancellationToken ct = default) => work.WriteAsync(async (db, token) =>
    {
        var item = await db.TelemetryEvents.SingleAsync(x => x.EventId == id, token);
        if (item.State != "Pending") return;
        item.Attempts++; item.State = exhausted ? "Failed" : "Pending"; item.NextAttemptUtc = nextAttempt.UtcDateTime; item.LastError = error[..Math.Min(error.Length, 200)];
        db.TelemetryFailures.Add(new() { EventId = id, TimestampUtc = DateTime.UtcNow, Error = item.LastError, Attempt = item.Attempts });
    }, ct);
    public async Task<TelemetryStatus> GetStatusAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var groups = await db.TelemetryEvents.AsNoTracking().GroupBy(x => x.State).Select(g => new { State = g.Key, Count = g.Count() }).ToListAsync(ct);
        int Count(string state) => groups.FirstOrDefault(g => g.State == state)?.Count ?? 0;
        return new(Count("Pending"), Count("Failed"), Count("Published"));
    }
    public async Task RetryAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct); var now = DateTime.UtcNow;
        await db.TelemetryEvents.Where(x => x.State == "Failed" || x.State == "Pending").ExecuteUpdateAsync(set => set.SetProperty(x => x.State, "Pending").SetProperty(x => x.Attempts, 0).SetProperty(x => x.NextAttemptUtc, now), ct);
    }
}
