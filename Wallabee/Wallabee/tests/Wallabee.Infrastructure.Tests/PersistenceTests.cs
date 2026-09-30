using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Wallabee.Application.Mapping;
using Wallabee.Contracts.Benchmark;
using Wallabee.Core.Configuration;
using Wallabee.Core.Enums;
using Wallabee.Core.Models;
using Wallabee.Infrastructure.Persistence;
using Wallabee.Infrastructure.Persistence.Repositories;
using Wallabee.Infrastructure.Quarantine;
using Wallabee.Infrastructure.FileSystem;
using Wallabee.Infrastructure.Hashing;
using Wallabee.Tests.Shared;
namespace Wallabee.Infrastructure.Tests;
public sealed class PersistenceTests : IAsyncLifetime
{
    private readonly SqlServerTestDatabase database = new();
    private EfScanResultRepository Repository => new(database, new(database), Options.Create(new PersistenceOptions()));
    private EfTelemetryQueue Queue => new(database, new(database));
    public Task InitializeAsync() => database.InitializeAsync();
    public async Task DisposeAsync() => await database.DisposeAsync();
    private static FileScanResult Result(string round) => new("lab/file.bin", 42, new string('A',64), 50.1234, DetectionLevel.Medium, [new("test", "indicator", "test indicator", 50.1234)], TimeSpan.FromMilliseconds(12.1234))
        { ExperimentId = "EXP", RoundId = round, SampleId = "sample", StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow };
    [Fact]
    public async Task PersistsNormalizedScanIndicatorsAndCorrelation()
    {
        var scan = Result("R1"); await Repository.SaveAsync(scan, scan.ToTelemetry());
        var other = Result("R2"); await Repository.SaveAsync(other, other.ToTelemetry());
        var loaded = (await Repository.GetAsync(scan.ScanId))!;
        Assert.Equal(scan.RiskScore, loaded.RiskScore); Assert.Equal(scan.Duration.TotalMilliseconds, loaded.Duration.TotalMilliseconds, 4);
        Assert.Single(loaded.Indicators); Assert.Equal(DetectionLevel.Medium, loaded.DetectionLevel);
        Assert.Equal(2, (await Repository.FindAsync(new(ExperimentId: "EXP"))).Count);
        Assert.Single(await Repository.FindAsync(new(ExperimentId: "EXP", RoundId: "R1")));
        Assert.Empty(await Repository.FindAsync(new(ExperimentId: "missing")));
        Assert.Equal(2, (await Repository.FindAsync(new(Sha256: new string('a',64)))).Count);
        await using var db = database.CreateDbContext();
        Assert.Equal(2, await db.ScanIndicators.CountAsync()); Assert.Equal(1, await db.Experiments.CountAsync()); Assert.Equal(2, await db.ExperimentRounds.CountAsync());
        Assert.Equal(2, await db.Samples.CountAsync()); Assert.All(await db.TelemetryEvents.ToListAsync(), e => Assert.Null(e.PayloadJson));
        Assert.Equal("Medium", await db.Database.SqlQueryRaw<string>("SELECT TOP(1) DetectionLevel AS Value FROM lab.Scans").SingleAsync());
        await Queue.AcknowledgeAsync(scan.ScanId);
        Assert.True((await db.Scans.AsNoTracking().SingleAsync(x => x.ScanId == scan.ScanId)).TelemetryPublished);
    }
    [Fact]
    public async Task ConstraintFailureRollsBackScanAndReferences()
    {
        var scan = Result("R1") with { Indicators = [new("test", new string('x',201), "too long", 1)] };
        await Assert.ThrowsAsync<DbUpdateException>(() => Repository.SaveAsync(scan, scan.ToTelemetry()));
        await using var db = database.CreateDbContext();
        Assert.Empty(await db.Scans.ToListAsync()); Assert.Empty(await db.Experiments.ToListAsync()); Assert.Empty(await db.ScanIndicators.ToListAsync()); Assert.Empty(await db.TelemetryEvents.ToListAsync());
    }
    [Fact]
    public async Task ForeignKeysRejectOrphanIndicators()
    {
        await using var db = database.CreateDbContext();
        db.ScanIndicators.Add(new() { ScanId = Guid.NewGuid(), IndicatorDefinitionId = -1, IndicatorId = Guid.NewGuid() });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    [Fact]
    public async Task BenchmarksUseStructuredColumnsAndSurviveNewContext()
    {
        var dto = new BenchmarkDto { BenchmarkId = Guid.NewGuid(), WallabeeVersion = "0.1.0", ExperimentId = "E", RoundId = "R", SamplesScanned = 2, SamplesDetected = 1,
            TruePositives = 1, TrueNegatives = 1, LabeledSamples = 2, Precision = 1, Recall = 1, F1Score = 1, P95ScanMs = 12.123456, CpuAverage = 2.125, RamPeakMb = 100.123,
            StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow };
        var json = JsonSerializer.Serialize(dto, ScanMapping.Json);
        var repo = new EfBenchmarkResultRepository(database, new(database), Options.Create(new PersistenceOptions()));
        await repo.SaveAsync(dto.BenchmarkId, "benchmark", json, [new(dto.BenchmarkId,"benchmark",json)]);
        var loaded = JsonSerializer.Deserialize<BenchmarkDto>((await repo.GetAsync(dto.BenchmarkId,"benchmark"))!, ScanMapping.Json)!;
        Assert.Equal(dto.P95ScanMs, loaded.P95ScanMs); Assert.Equal(dto.Precision, loaded.Precision); Assert.Equal(dto.ExperimentId, loaded.ExperimentId);
        await using var db = database.CreateDbContext(); var row = await db.BenchmarkRuns.SingleAsync(); Assert.Equal(12.123456m,row.P95ScanMs); Assert.Null(row.ConfigurationJson);
    }
    [Fact]
    public async Task RetryFailureHistoryAndHealthUseSqlServer()
    {
        var id = Guid.NewGuid(); await Queue.EnqueueAsync(new(id,"system","{}"));
        await Queue.FailAsync(id,"HttpRequestException",DateTimeOffset.UtcNow.AddHours(1),false);
        Assert.Empty(await Queue.GetDueAsync(10,DateTimeOffset.UtcNow));
        await Queue.FailAsync(id,"HttpRequestException",DateTimeOffset.UtcNow,true);
        Assert.Equal(1,(await Queue.GetStatusAsync()).Failed); await Queue.RetryAsync(); Assert.Single(await Queue.GetDueAsync(10,DateTimeOffset.UtcNow));
        await using var db = database.CreateDbContext(); Assert.Equal(2,await db.TelemetryFailures.CountAsync());
        Assert.Equal(HealthStatus.Healthy,(await new SqlServerHealthCheck(database).CheckHealthAsync(new())).Status);
    }
    [Fact]
    public async Task QuarantineMetadataSurvivesServiceRestart()
    {
        var root = Path.Combine(Path.GetTempPath(),"wallabee-quarantine-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            var file = Path.Combine(root,"inert.txt"); await File.WriteAllTextAsync(file,"inert sample");
            LocalQuarantineService Service() => new(new(root),new Sha256HashCalculator(), Options.Create(new QuarantineOptions { Directory=Path.Combine(root,"quarantine"), AllowRestore=true }),new EfQuarantineRepository(database,new(database)),NullLogger<LocalQuarantineService>.Instance);
            var entry = await Service().QuarantineAsync(file); Assert.False(File.Exists(file)); await Service().RestoreAsync(entry.Id);
            Assert.Equal("inert sample",await File.ReadAllTextAsync(file)); Assert.NotNull(Assert.Single(await Service().ListAsync()).RestoredAt);
        }
        finally { Directory.Delete(root,true); }
    }
    [Fact]
    public async Task ConcurrentScansDoNotShareDbContextsOrDuplicateReferences()
    {
        var results = Enumerable.Range(0,4).Select(_=>Result("R1")).ToArray();
        await Task.WhenAll(results.Select(r=>Repository.SaveAsync(r,r.ToTelemetry())));
        await using var db=database.CreateDbContext(); Assert.Equal(4,await db.Scans.CountAsync()); Assert.Equal(1,await db.Experiments.CountAsync()); Assert.Equal(1,await db.Samples.CountAsync());
    }
    [Fact]
    public async Task VersionContractAndPhysicalIndexesMatchModel()
    {
        var scan = Result("R1") with { HoneyBadgerVersion = new string('v',80) };
        await Repository.SaveAsync(scan,scan.ToTelemetry());
        Assert.Equal(scan.HoneyBadgerVersion,(await Repository.GetAsync(scan.ScanId))!.HoneyBadgerVersion);
        await using var db=database.CreateDbContext();
        var names=await db.Database.SqlQueryRaw<string>("SELECT name AS Value FROM sys.indexes WHERE name IS NOT NULL").ToListAsync();
        foreach(var entity in db.Model.GetEntityTypes()) foreach(var index in entity.GetIndexes()) Assert.Contains(index.GetDatabaseName()!,names);
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
    }
    [Fact]
    public void ModelUsesExplicitDecimalPrecisionAndNoBinaryContent()
    {
        using var db=database.CreateDbContext();
        foreach(var entity in db.Model.GetEntityTypes()) foreach(var p in entity.GetProperties())
        { Assert.NotEqual(typeof(byte[]),p.ClrType); if(p.ClrType==typeof(decimal)||p.ClrType==typeof(decimal?)) { Assert.NotNull(p.GetPrecision()); Assert.NotNull(p.GetScale()); } }
        var scan=db.Model.FindEntityType(typeof(Wallabee.Infrastructure.Persistence.Entities.ScanEntity))!;
        Assert.Equal("char(64)",scan.FindProperty("Sha256")!.GetColumnType());
        Assert.Equal("varchar(30)",scan.FindProperty("DetectionLevel")!.GetColumnType());
    }
}
