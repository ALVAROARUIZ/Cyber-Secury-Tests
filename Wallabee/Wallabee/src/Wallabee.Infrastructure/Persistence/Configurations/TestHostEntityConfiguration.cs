using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallabee.Infrastructure.Persistence.Entities;
namespace Wallabee.Infrastructure.Persistence.Configurations;
public sealed class TestHostEntityConfiguration : IEntityTypeConfiguration<TestHostEntity>
{
    public void Configure(EntityTypeBuilder<TestHostEntity> b)
    {
        b.ToTable("TestHosts", "lab");
        b.HasKey(e => e.TestHostId);
        b.Property(e => e.TestHostId).HasColumnType("int").IsRequired().UseIdentityColumn();
        b.Property(e => e.HostName).HasColumnType("nvarchar(150)").HasMaxLength(150).IsRequired();
        b.Property(e => e.HostAlias).HasColumnType("nvarchar(150)").HasMaxLength(150);
        b.Property(e => e.OperatingSystem).HasColumnType("nvarchar(150)").HasMaxLength(150);
        b.Property(e => e.OperatingSystemVersion).HasColumnType("nvarchar(100)").HasMaxLength(100);
        b.Property(e => e.Architecture).HasColumnType("varchar(50)").HasMaxLength(50).IsUnicode(false);
        b.Property(e => e.CpuModel).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.CpuLogicalCores).HasColumnType("int");
        b.Property(e => e.CpuPhysicalCores).HasColumnType("int");
        b.Property(e => e.TotalMemoryMb).HasColumnType("bigint");
        b.Property(e => e.GpuModel).HasColumnType("nvarchar(200)").HasMaxLength(200);
        b.Property(e => e.StorageType).HasColumnType("nvarchar(100)").HasMaxLength(100);
        b.Property(e => e.IsVirtualMachine).HasColumnType("bit").IsRequired();
        b.Property(e => e.Hypervisor).HasColumnType("nvarchar(100)").HasMaxLength(100);
        b.Property(e => e.EnvironmentName).HasColumnType("nvarchar(100)").HasMaxLength(100);
        b.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(e => e.LastSeenAt).HasColumnType("datetime2(3)");
        b.Property(e => e.IsActive).HasColumnType("bit").IsRequired();
    }
}
