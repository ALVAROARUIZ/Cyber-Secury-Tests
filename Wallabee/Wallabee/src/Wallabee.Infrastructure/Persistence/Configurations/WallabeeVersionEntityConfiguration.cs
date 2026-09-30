using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class WallabeeVersionEntityConfiguration : IEntityTypeConfiguration<WallabeeVersionEntity>
{
    public void Configure(EntityTypeBuilder<WallabeeVersionEntity> b)
    {
        b.ToTable("WallabeeVersions", "lab");
        b.HasKey(e => e.WallabeeVersionId);
        b.Property(e => e.WallabeeVersionId).HasColumnType("int").IsRequired().UseIdentityColumn();
        b.Property(e => e.Version).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        b.Property(e => e.MajorVersion).HasColumnType("int");
        b.Property(e => e.MinorVersion).HasColumnType("int");
        b.Property(e => e.PatchVersion).HasColumnType("int");
        b.Property(e => e.BuildMetadata).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        b.Property(e => e.CommitHash).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        b.Property(e => e.BranchName).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        b.Property(e => e.BuildDate).HasColumnType("datetime2(3)");
        b.Property(e => e.Description).HasColumnType("nvarchar(500)").HasMaxLength(500);
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasAlternateKey(e => e.Version).HasName("UQ_WallabeeVersions_Version");
    }
}
