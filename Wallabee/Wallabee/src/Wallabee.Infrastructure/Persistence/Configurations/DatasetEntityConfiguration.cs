using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class DatasetEntityConfiguration : IEntityTypeConfiguration<DatasetEntity>
{
    public void Configure(EntityTypeBuilder<DatasetEntity> b)
    {
        b.ToTable("Datasets", "lab");
        b.HasKey(e => e.DatasetId);
        b.Property(e => e.DatasetId).HasColumnType("uniqueidentifier").IsRequired().ValueGeneratedNever();
        b.Property(e => e.DatasetName).HasColumnType("nvarchar(200)").HasMaxLength(200).IsRequired();
        b.Property(e => e.DatasetVersion).HasColumnType("varchar(50)").HasMaxLength(50).IsUnicode(false);
        b.Property(e => e.Description).HasColumnType("nvarchar(1000)").HasMaxLength(1000);
        b.Property(e => e.Seed).HasColumnType("bigint");
        b.Property(e => e.TotalSamples).HasColumnType("int");
        b.Property(e => e.BenignSamples).HasColumnType("int");
        b.Property(e => e.SyntheticSamples).HasColumnType("int");
        b.Property(e => e.ManifestSha256).HasColumnType("char(64)").HasMaxLength(64).IsUnicode(false).IsFixedLength();
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
