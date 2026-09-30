using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class TelemetryEventEntityConfiguration : IEntityTypeConfiguration<TelemetryEventEntity>
{
    public void Configure(EntityTypeBuilder<TelemetryEventEntity> b)
    {
        b.ToTable("TelemetryEvents", "telemetry");
        b.HasKey(e => e.TelemetryEventId);
        b.Property(e => e.TelemetryEventId).HasColumnType("bigint").IsRequired().UseIdentityColumn();
        b.Property(e => e.EventId).HasColumnType("uniqueidentifier").IsRequired();
        b.Property(e => e.ExperimentId).HasColumnType("uniqueidentifier");
        b.Property(e => e.RoundId).HasColumnType("uniqueidentifier");
        b.Property(e => e.SampleId).HasColumnType("uniqueidentifier");
        b.Property(e => e.ScanId).HasColumnType("uniqueidentifier");
        b.Property(e => e.BenchmarkId).HasColumnType("uniqueidentifier");
        b.Property(e => e.SourceComponent).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        b.Property(e => e.EventType).HasColumnType("varchar(150)").HasMaxLength(150).IsUnicode(false).IsRequired();
        b.Property(e => e.Severity).HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false);
        b.Property(e => e.TimestampUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.Message).HasColumnType("nvarchar(1000)").HasMaxLength(1000);
        b.Property(e => e.PayloadJson).HasColumnType("nvarchar(4000)").HasMaxLength(4000);
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(e => e.State).HasColumnType("varchar(20)").HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.Attempts).HasColumnType("int").IsRequired();
        b.Property(e => e.NextAttemptUtc).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.LastError).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.HasOne<ExperimentEntity>().WithMany().HasForeignKey(e => e.ExperimentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_TelemetryEvents_Experiment");
        b.HasOne<ExperimentRoundEntity>().WithMany().HasForeignKey(e => e.RoundId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_TelemetryEvents_Round");
        b.HasOne<SampleEntity>().WithMany().HasForeignKey(e => e.SampleId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_TelemetryEvents_Sample");
        b.HasOne<ScanEntity>().WithMany().HasForeignKey(e => e.ScanId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_TelemetryEvents_Scan");
        b.HasOne<BenchmarkResultEntity>().WithMany().HasForeignKey(e => e.BenchmarkId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_TelemetryEvents_Benchmark");
        b.HasAlternateKey(e => e.EventId).HasName("UQ_TelemetryEvents_EventId");
        b.HasIndex(e => new { e.ExperimentId, e.TimestampUtc }).HasDatabaseName("IX_TelemetryEvents_Experiment_Timestamp");
        b.HasIndex(e => new { e.SourceComponent, e.EventType, e.TimestampUtc }).HasDatabaseName("IX_TelemetryEvents_Source_Event_Timestamp");
        b.HasIndex(e => new { e.State, e.NextAttemptUtc });
    }
}
