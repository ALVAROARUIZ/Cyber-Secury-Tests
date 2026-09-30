using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class ExperimentEventEntityConfiguration : IEntityTypeConfiguration<ExperimentEventEntity>
{
    public void Configure(EntityTypeBuilder<ExperimentEventEntity> b)
    {
        b.ToTable("ExperimentEvents", "lab");
        b.HasKey(e => e.ExperimentEventId);
        b.Property(e => e.ExperimentEventId).HasColumnType("bigint").IsRequired().UseIdentityColumn();
        b.Property(e => e.ExperimentId).HasColumnType("uniqueidentifier").IsRequired();
        b.Property(e => e.RoundId).HasColumnType("uniqueidentifier");
        b.Property(e => e.EventType).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        b.Property(e => e.Message).HasColumnType("nvarchar(1000)").HasMaxLength(1000);
        b.Property(e => e.OccurredAt).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.MetadataJson).HasColumnType("nvarchar(max)");
        b.HasOne<ExperimentEntity>().WithMany().HasForeignKey(e => e.ExperimentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ExperimentEvents_Experiment");
        b.HasOne<ExperimentRoundEntity>().WithMany().HasForeignKey(e => e.RoundId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ExperimentEvents_Round");
        b.HasIndex(e => new { e.ExperimentId, e.OccurredAt }).HasDatabaseName("IX_ExperimentEvents_Experiment_OccurredAt");
    }
}
