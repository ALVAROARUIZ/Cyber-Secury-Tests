using Microsoft.EntityFrameworkCore;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence;
public sealed class DiveViewDbContext(DbContextOptions<DiveViewDbContext> options) : DbContext(options)
{
    public DbSet<WallabeeVersionEntity> WallabeeVersions => Set<WallabeeVersionEntity>();
    public DbSet<HoneyBadgerVersionEntity> HoneyBadgerVersions => Set<HoneyBadgerVersionEntity>();
    public DbSet<TestHostEntity> TestHosts => Set<TestHostEntity>();
    public DbSet<DatasetEntity> Datasets => Set<DatasetEntity>();
    public DbSet<ExperimentEntity> Experiments => Set<ExperimentEntity>();
    public DbSet<ExperimentRoundEntity> ExperimentRounds => Set<ExperimentRoundEntity>();
    public DbSet<SampleEntity> Samples => Set<SampleEntity>();
    public DbSet<ScanEntity> Scans => Set<ScanEntity>();
    public DbSet<IndicatorDefinitionEntity> IndicatorDefinitions => Set<IndicatorDefinitionEntity>();
    public DbSet<ThreatIndicatorEntity> ScanIndicators => Set<ThreatIndicatorEntity>();
    public DbSet<BenchmarkResultEntity> BenchmarkRuns => Set<BenchmarkResultEntity>();
    public DbSet<SystemMetricEntity> SystemMetricSamples => Set<SystemMetricEntity>();
    public DbSet<TelemetryEventEntity> TelemetryEvents => Set<TelemetryEventEntity>();
    public DbSet<QuarantineEventEntity> QuarantineEvents => Set<QuarantineEventEntity>();
    public DbSet<ExperimentEventEntity> ExperimentEvents => Set<ExperimentEventEntity>();
    public DbSet<DetectionConfusionMatrixEntity> DetectionConfusionMatrix => Set<DetectionConfusionMatrixEntity>();
    public DbSet<TelemetryFailureEntity> TelemetryFailures => Set<TelemetryFailureEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(typeof(DiveViewDbContext).Assembly);
}
