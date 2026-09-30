using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class HoneyBadgerVersionEntityConfiguration : IEntityTypeConfiguration<HoneyBadgerVersionEntity>
{
    public void Configure(EntityTypeBuilder<HoneyBadgerVersionEntity> b)
    {
        b.ToTable("HoneyBadgerVersions", "lab");
        b.HasKey(e => e.HoneyBadgerVersionId);
        b.Property(e => e.HoneyBadgerVersionId).HasColumnType("int").IsRequired().UseIdentityColumn();
        b.Property(e => e.Version).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        b.Property(e => e.MajorVersion).HasColumnType("int");
        b.Property(e => e.MinorVersion).HasColumnType("int");
        b.Property(e => e.PatchVersion).HasColumnType("int");
        b.Property(e => e.CommitHash).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        b.Property(e => e.BranchName).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        b.Property(e => e.BuildDate).HasColumnType("datetime2(3)");
        b.Property(e => e.Description).HasColumnType("nvarchar(500)").HasMaxLength(500);
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasAlternateKey(e => e.Version).HasName("UQ_HoneyBadgerVersions_Version");
    }
}
