using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class QuarantineEventEntityConfiguration : IEntityTypeConfiguration<QuarantineEventEntity>
{
    public void Configure(EntityTypeBuilder<QuarantineEventEntity> b)
    {
        b.ToTable("QuarantineEvents", "lab");
        b.HasKey(e => e.QuarantineEventId);
        b.Property(e => e.QuarantineEventId).HasColumnType("uniqueidentifier").IsRequired().ValueGeneratedNever();
        b.Property(e => e.ScanId).HasColumnType("uniqueidentifier");
        b.Property(e => e.SampleId).HasColumnType("uniqueidentifier");
        b.Property(e => e.Sha256).HasColumnType("char(64)").HasMaxLength(64).IsUnicode(false).IsFixedLength().IsRequired();
        b.Property(e => e.Action).HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(e => e.Reason).HasColumnType("nvarchar(500)").HasMaxLength(500);
        b.Property(e => e.QuarantineReference).HasColumnType("nvarchar(500)").HasMaxLength(500);
        b.Property(e => e.OccurredAt).HasColumnType("datetime2(3)").IsRequired();
        b.Property(e => e.OriginalPath).HasColumnType("nvarchar(2048)").HasMaxLength(2048);
        b.Property(e => e.QuarantinePath).HasColumnType("nvarchar(2048)").HasMaxLength(2048);
        b.Property(e => e.RestoredAt).HasColumnType("datetime2(3)");
        b.Property(e => e.State).HasColumnType("varchar(20)").HasMaxLength(20).IsUnicode(false).IsRequired();
        b.HasOne<ScanEntity>().WithMany().HasForeignKey(e => e.ScanId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_QuarantineEvents_Scan");
        b.HasOne<SampleEntity>().WithMany().HasForeignKey(e => e.SampleId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_QuarantineEvents_Sample");
    }
}
