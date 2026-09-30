using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class ThreatIndicatorEntityConfiguration : IEntityTypeConfiguration<ThreatIndicatorEntity>
{
    public void Configure(EntityTypeBuilder<ThreatIndicatorEntity> b)
    {
        b.ToTable("ScanIndicators", "lab");
        b.HasKey(e => e.ScanIndicatorId);
        b.Property(e => e.ScanIndicatorId).HasColumnType("bigint").IsRequired().UseIdentityColumn();
        b.Property(e => e.ScanId).HasColumnType("uniqueidentifier").IsRequired();
        b.Property(e => e.IndicatorDefinitionId).HasColumnType("int").IsRequired();
        b.Property(e => e.WeightApplied).HasColumnType("decimal(9,4)").HasPrecision(9, 4);
        b.Property(e => e.ObservedValue).HasColumnType("nvarchar(500)").HasMaxLength(500);
        b.Property(e => e.Triggered).HasColumnType("bit").IsRequired();
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(e => e.IndicatorId).HasColumnType("uniqueidentifier").IsRequired();
        b.Property(e => e.Source).HasColumnType("nvarchar(100)").HasMaxLength(100);
        b.Property(e => e.Name).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.Description).HasColumnType("nvarchar(1000)").HasMaxLength(1000);
        b.Property(e => e.Category).HasColumnType("varchar(50)").HasMaxLength(50).IsUnicode(false);
        b.HasOne<ScanEntity>(e => e.Scan).WithMany(e => e.Indicators).HasForeignKey(e => e.ScanId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ScanIndicators_Scan");
        b.HasOne<IndicatorDefinitionEntity>(e => e.Definition).WithMany().HasForeignKey(e => e.IndicatorDefinitionId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ScanIndicators_Definition");
        b.HasAlternateKey(e => new { e.ScanId, e.IndicatorDefinitionId }).HasName("UQ_ScanIndicators_Scan_Indicator");
        b.HasIndex(e => new { e.ScanId, e.Triggered }).HasDatabaseName("IX_ScanIndicators_Scan_Triggered");
        b.HasIndex(e => new { e.IndicatorDefinitionId, e.Triggered }).HasDatabaseName("IX_ScanIndicators_Definition_Triggered");
        b.HasIndex(e => e.IndicatorId).IsUnique();
    }
}
