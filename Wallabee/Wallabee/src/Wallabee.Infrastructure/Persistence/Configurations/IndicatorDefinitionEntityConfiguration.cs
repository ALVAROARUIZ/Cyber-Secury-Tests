using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class IndicatorDefinitionEntityConfiguration : IEntityTypeConfiguration<IndicatorDefinitionEntity>
{
    public void Configure(EntityTypeBuilder<IndicatorDefinitionEntity> b)
    {
        b.ToTable("IndicatorDefinitions", "lab");
        b.HasKey(e => e.IndicatorDefinitionId);
        b.Property(e => e.IndicatorDefinitionId).HasColumnType("int").IsRequired().UseIdentityColumn();
        b.Property(e => e.IndicatorCode).HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        b.Property(e => e.IndicatorName).HasColumnType("nvarchar(200)").HasMaxLength(200).IsRequired();
        b.Property(e => e.Source).HasColumnType("nvarchar(100)").HasMaxLength(100).IsRequired();
        b.Property(e => e.Description).HasColumnType("nvarchar(1000)").HasMaxLength(1000);
        b.Property(e => e.DefaultWeight).HasColumnType("decimal(9,4)").HasPrecision(9, 4);
        b.Property(e => e.Category).HasColumnType("nvarchar(100)").HasMaxLength(100);
        b.Property(e => e.IsActive).HasColumnType("bit").IsRequired();
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasAlternateKey(e => e.IndicatorCode).HasName("UQ_IndicatorDefinitions_Code");
    }
}
