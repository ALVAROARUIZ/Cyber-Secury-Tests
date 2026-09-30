using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class ExperimentRoundEntityConfiguration : IEntityTypeConfiguration<ExperimentRoundEntity>
{
    public void Configure(EntityTypeBuilder<ExperimentRoundEntity> b)
    {
        b.ToTable("ExperimentRounds", "lab");
        b.HasKey(e => e.RoundId);
        b.Property(e => e.RoundId).HasColumnType("uniqueidentifier").IsRequired().ValueGeneratedNever();
        b.Property(e => e.ExperimentId).HasColumnType("uniqueidentifier").IsRequired();
        b.Property(e => e.ExternalRoundId).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.RoundNumber).HasColumnType("int").IsRequired();
        b.Property(e => e.GenerationNumber).HasColumnType("int");
        b.Property(e => e.StartedAt).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.CompletedAt).HasColumnType("datetime2(3)");
        b.Property(e => e.SamplesGenerated).HasColumnType("int").IsRequired();
        b.Property(e => e.SamplesScanned).HasColumnType("int").IsRequired();
        b.Property(e => e.SamplesDetected).HasColumnType("int").IsRequired();
        b.Property(e => e.SamplesMissed).HasColumnType("int").IsRequired();
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
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne<ExperimentEntity>().WithMany().HasForeignKey(e => e.ExperimentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ExperimentRounds_Experiment");
        b.HasAlternateKey(e => new { e.ExperimentId, e.RoundNumber }).HasName("UQ_ExperimentRounds_Experiment_Round");
        b.HasIndex(e => new { e.ExperimentId, e.GenerationNumber, e.StartedAt }).HasDatabaseName("IX_ExperimentRounds_Experiment_Generation_StartedAt");
        b.HasIndex(e => new { e.ExperimentId, e.ExternalRoundId }).IsUnique().HasFilter("[ExternalRoundId] IS NOT NULL");
    }
}
