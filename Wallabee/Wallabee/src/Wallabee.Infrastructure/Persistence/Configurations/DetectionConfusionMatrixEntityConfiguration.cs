using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class DetectionConfusionMatrixEntityConfiguration : IEntityTypeConfiguration<DetectionConfusionMatrixEntity>
{
    public void Configure(EntityTypeBuilder<DetectionConfusionMatrixEntity> b)
    {
        b.ToTable("DetectionConfusionMatrix", "benchmark");
        b.HasKey(e => e.DetectionConfusionMatrixId);
        b.Property(e => e.DetectionConfusionMatrixId).HasColumnType("bigint").IsRequired().UseIdentityColumn();
        b.Property(e => e.ExperimentId).HasColumnType("uniqueidentifier").IsRequired();
        b.Property(e => e.RoundId).HasColumnType("uniqueidentifier");
        b.Property(e => e.BenchmarkId).HasColumnType("uniqueidentifier");
        b.Property(e => e.TruePositives).HasColumnType("int").IsRequired();
        b.Property(e => e.TrueNegatives).HasColumnType("int").IsRequired();
        b.Property(e => e.FalsePositives).HasColumnType("int").IsRequired();
        b.Property(e => e.FalseNegatives).HasColumnType("int").IsRequired();
        b.Property(e => e.PrecisionScore).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.RecallScore).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.SpecificityScore).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.F1Score).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.AccuracyScore).HasColumnType("decimal(9,6)").HasPrecision(9, 6);
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne<ExperimentEntity>().WithMany().HasForeignKey(e => e.ExperimentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_DetectionConfusionMatrix_Experiment");
        b.HasOne<ExperimentRoundEntity>().WithMany().HasForeignKey(e => e.RoundId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_DetectionConfusionMatrix_Round");
        b.HasOne<BenchmarkResultEntity>().WithMany().HasForeignKey(e => e.BenchmarkId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_DetectionConfusionMatrix_Benchmark");
    }
}
