using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class SystemMetricEntityConfiguration : IEntityTypeConfiguration<SystemMetricEntity>
{
    public void Configure(EntityTypeBuilder<SystemMetricEntity> b)
    {
        b.ToTable("SystemMetricSamples", "telemetry");
        b.HasKey(e => e.SystemMetricSampleId);
        b.Property(e => e.SystemMetricSampleId).HasColumnType("bigint").IsRequired().UseIdentityColumn();
        b.Property(e => e.ExperimentId).HasColumnType("uniqueidentifier");
        b.Property(e => e.RoundId).HasColumnType("uniqueidentifier");
        b.Property(e => e.BenchmarkId).HasColumnType("uniqueidentifier");
        b.Property(e => e.TestHostId).HasColumnType("int");
        b.Property(e => e.TimestampUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.CpuPercent).HasColumnType("decimal(7,3)").HasPrecision(7, 3);
        b.Property(e => e.MemoryUsedMb).HasColumnType("decimal(18,3)").HasPrecision(18, 3);
        b.Property(e => e.MemoryPercent).HasColumnType("decimal(7,3)").HasPrecision(7, 3);
        b.Property(e => e.DiskReadBytesPerSecond).HasColumnType("bigint");
        b.Property(e => e.DiskWriteBytesPerSecond).HasColumnType("bigint");
        b.Property(e => e.DiskReadOperationsPerSecond).HasColumnType("decimal(18,3)").HasPrecision(18, 3);
        b.Property(e => e.DiskWriteOperationsPerSecond).HasColumnType("decimal(18,3)").HasPrecision(18, 3);
        b.Property(e => e.ProcessCpuPercent).HasColumnType("decimal(7,3)").HasPrecision(7, 3);
        b.Property(e => e.ProcessMemoryMb).HasColumnType("decimal(18,3)").HasPrecision(18, 3);
        b.Property(e => e.ThreadCount).HasColumnType("int");
        b.Property(e => e.HandleCount).HasColumnType("int");
        b.Property(e => e.CpuPeakPercent).HasColumnType("decimal(7,3)").HasPrecision(7, 3);
        b.Property(e => e.MemoryPeakMb).HasColumnType("decimal(18,3)").HasPrecision(18, 3);
        b.HasOne<ExperimentEntity>().WithMany().HasForeignKey(e => e.ExperimentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_SystemMetricSamples_Experiment");
        b.HasOne<ExperimentRoundEntity>().WithMany().HasForeignKey(e => e.RoundId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_SystemMetricSamples_Round");
        b.HasOne<BenchmarkResultEntity>().WithMany().HasForeignKey(e => e.BenchmarkId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_SystemMetricSamples_Benchmark");
        b.HasOne<TestHostEntity>().WithMany().HasForeignKey(e => e.TestHostId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_SystemMetricSamples_TestHost");
        b.HasIndex(e => new { e.BenchmarkId, e.TimestampUtc }).HasDatabaseName("IX_SystemMetricSamples_Benchmark_Timestamp");
        b.HasIndex(e => new { e.ExperimentId, e.RoundId, e.TimestampUtc }).HasDatabaseName("IX_SystemMetricSamples_Experiment_Round_Timestamp");
    }
}
