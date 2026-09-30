using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class ScanEntityConfiguration : IEntityTypeConfiguration<ScanEntity>
{
    public void Configure(EntityTypeBuilder<ScanEntity> b)
    {
        b.ToTable("Scans", "lab");
        b.HasKey(e => e.ScanId);
        b.Property(e => e.ScanId).HasColumnType("uniqueidentifier").IsRequired().ValueGeneratedNever();
        b.Property(e => e.SampleId).HasColumnType("uniqueidentifier");
        b.Property(e => e.ExperimentId).HasColumnType("uniqueidentifier");
        b.Property(e => e.RoundId).HasColumnType("uniqueidentifier");
        b.Property(e => e.WallabeeVersionId).HasColumnType("int").IsRequired();
        b.Property(e => e.TestHostId).HasColumnType("int");
        b.Property(e => e.Sha256).HasColumnType("char(64)").HasMaxLength(64).IsUnicode(false).IsFixedLength().IsRequired();
        b.Property(e => e.FileName).HasColumnType("nvarchar(260)").HasMaxLength(260);
        b.Property(e => e.FileSizeBytes).HasColumnType("bigint").IsRequired();
        b.Property(e => e.RiskScore).HasColumnType("decimal(9,4)").HasPrecision(9, 4).IsRequired();
        b.Property(e => e.DetectionLevel).HasColumnType("varchar(30)").HasConversion<string>().IsRequired();
        b.Property(e => e.WasDetected).HasColumnType("bit").IsRequired();
        b.Property(e => e.StartedAt).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.CompletedAt).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.DurationMs).HasColumnType("decimal(18,6)").HasPrecision(18, 6).IsRequired();
        b.Property(e => e.BytesPerSecond).HasColumnType("decimal(18,4)").HasPrecision(18, 4);
        b.Property(e => e.ErrorOccurred).HasColumnType("bit").IsRequired();
        b.Property(e => e.ErrorCode).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        b.Property(e => e.ErrorMessage).HasColumnType("nvarchar(1000)").HasMaxLength(1000);
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(e => e.HoneyBadgerVersionId).HasColumnType("int");
        b.Property(e => e.ExternalExperimentId).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.ExternalRoundId).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.ExternalSampleId).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.ResponseAction).HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(e => e.RetainSample).HasColumnType("bit").IsRequired();
        b.Property(e => e.RetainReason).HasColumnType("varchar(50)").HasMaxLength(50).IsUnicode(false);
        b.Property(e => e.TelemetryPublished).HasColumnType("bit").IsRequired();
        b.HasOne<SampleEntity>().WithMany().HasForeignKey(e => e.SampleId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Scans_Sample");
        b.HasOne<ExperimentEntity>().WithMany().HasForeignKey(e => e.ExperimentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Scans_Experiment");
        b.HasOne<ExperimentRoundEntity>().WithMany().HasForeignKey(e => e.RoundId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Scans_Round");
        b.HasOne<WallabeeVersionEntity>(e => e.WallabeeVersion).WithMany().HasForeignKey(e => e.WallabeeVersionId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Scans_WallabeeVersion");
        b.HasOne<TestHostEntity>().WithMany().HasForeignKey(e => e.TestHostId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Scans_TestHost");
        b.HasOne(e => e.HoneyBadgerVersion).WithMany().HasForeignKey(e => e.HoneyBadgerVersionId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(e => e.Sha256).HasDatabaseName("IX_Scans_Sha256");
        b.HasIndex(e => e.SampleId).HasDatabaseName("IX_Scans_Sample");
        b.HasIndex(e => new { e.ExperimentId, e.RoundId }).HasDatabaseName("IX_Scans_Experiment_Round");
        b.HasIndex(e => new { e.WallabeeVersionId, e.StartedAt }).HasDatabaseName("IX_Scans_Wallabee_StartedAt");
        b.HasIndex(e => new { e.WallabeeVersionId, e.DetectionLevel }).HasDatabaseName("IX_Scans_Wallabee_DetectionLevel");
        b.HasIndex(e => new { e.ExperimentId, e.WasDetected }).HasDatabaseName("IX_Scans_Experiment_Detected");
        b.HasIndex(e => e.StartedAt);
        b.HasIndex(e => e.DetectionLevel);
        b.HasIndex(e => e.WasDetected);
        b.HasIndex(e => e.RoundId);
    }
}
