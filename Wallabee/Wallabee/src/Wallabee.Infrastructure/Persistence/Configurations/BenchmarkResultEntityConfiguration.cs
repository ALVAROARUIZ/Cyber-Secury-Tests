using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class BenchmarkResultEntityConfiguration : IEntityTypeConfiguration<BenchmarkResultEntity>
{
    public void Configure(EntityTypeBuilder<BenchmarkResultEntity> b)
    {
        b.ToTable("BenchmarkRuns", "benchmark");
        b.HasKey(e => e.BenchmarkId);
        b.Property(e => e.BenchmarkId).HasColumnType("uniqueidentifier").IsRequired().ValueGeneratedNever();
        b.Property(e => e.ExperimentId).HasColumnType("uniqueidentifier");
        b.Property(e => e.RoundId).HasColumnType("uniqueidentifier");
        b.Property(e => e.WallabeeVersionId).HasColumnType("int").IsRequired();
        b.Property(e => e.HoneyBadgerVersionId).HasColumnType("int");
        b.Property(e => e.TestHostId).HasColumnType("int");
        b.Property(e => e.BenchmarkType).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        b.Property(e => e.StartedAt).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.CompletedAt).HasColumnType("datetime2(3)");
        b.Property(e => e.DurationMs).HasColumnType("decimal(18,6)").HasPrecision(18, 6);
        b.Property(e => e.SamplesScanned).HasColumnType("int").IsRequired();
        b.Property(e => e.SamplesDetected).HasColumnType("int");
        b.Property(e => e.SamplesMissed).HasColumnType("int");
        b.Property(e => e.TruePositives).HasColumnType("int");
        b.Property(e => e.TrueNegatives).HasColumnType("int");
        b.Property(e => e.FalsePositives).HasColumnType("int");
        b.Property(e => e.FalseNegatives).HasColumnType("int");
        b.Property(e => e.DetectionRate).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.EvasionRate).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.PrecisionScore).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.RecallScore).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.F1Score).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.AverageRiskScore).HasColumnType("decimal(9,4)").HasPrecision(9, 4);
        b.Property(e => e.AverageScanMs).HasColumnType("decimal(18,6)").HasPrecision(18, 6);
        b.Property(e => e.P50ScanMs).HasColumnType("decimal(18,6)").HasPrecision(18, 6);
        b.Property(e => e.P95ScanMs).HasColumnType("decimal(18,6)").HasPrecision(18, 6);
        b.Property(e => e.P99ScanMs).HasColumnType("decimal(18,6)").HasPrecision(18, 6);
        b.Property(e => e.FilesPerSecond).HasColumnType("decimal(18,4)").HasPrecision(18, 4);
        b.Property(e => e.MbPerSecond).HasColumnType("decimal(18,4)").HasPrecision(18, 4);
        b.Property(e => e.CpuAveragePercent).HasColumnType("decimal(7,3)").HasPrecision(7, 3);
        b.Property(e => e.CpuPeakPercent).HasColumnType("decimal(7,3)").HasPrecision(7, 3);
        b.Property(e => e.MemoryAverageMb).HasColumnType("decimal(18,3)").HasPrecision(18, 3);
        b.Property(e => e.MemoryPeakMb).HasColumnType("decimal(18,3)").HasPrecision(18, 3);
        b.Property(e => e.ConfigurationJson).HasColumnType("nvarchar(max)");
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(e => e.LabeledSamples).HasColumnType("int").IsRequired();
        b.Property(e => e.FalsePositiveRate).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.FalseNegativeRate).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.BytesPerSecond).HasColumnType("decimal(18,4)").HasPrecision(18, 4).IsRequired();
        b.Property(e => e.SamplesGenerated).HasColumnType("int");
        b.Property(e => e.ExternalExperimentId).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.ExternalRoundId).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.HasOne<ExperimentEntity>().WithMany().HasForeignKey(e => e.ExperimentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_BenchmarkRuns_Experiment");
        b.HasOne<ExperimentRoundEntity>().WithMany().HasForeignKey(e => e.RoundId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_BenchmarkRuns_Round");
        b.HasOne<WallabeeVersionEntity>(e => e.WallabeeVersion).WithMany().HasForeignKey(e => e.WallabeeVersionId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_BenchmarkRuns_WallabeeVersion");
        b.HasOne<HoneyBadgerVersionEntity>(e => e.HoneyBadgerVersion).WithMany().HasForeignKey(e => e.HoneyBadgerVersionId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_BenchmarkRuns_HoneyBadgerVersion");
        b.HasOne<TestHostEntity>().WithMany().HasForeignKey(e => e.TestHostId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_BenchmarkRuns_TestHost");
        b.HasIndex(e => new { e.WallabeeVersionId, e.StartedAt }).HasDatabaseName("IX_BenchmarkRuns_Wallabee_StartedAt");
        b.HasIndex(e => new { e.ExperimentId, e.RoundId }).HasDatabaseName("IX_BenchmarkRuns_Experiment_Round");
    }
}
