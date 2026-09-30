using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class TestHostEntity
{
    public int TestHostId { get; set; }
    public string HostName { get; set; } = null!;
    public string? HostAlias { get; set; }
    public string? OperatingSystem { get; set; }
    public string? OperatingSystemVersion { get; set; }
    public string? Architecture { get; set; }
    public string? CpuModel { get; set; }
    public int? CpuLogicalCores { get; set; }
    public int? CpuPhysicalCores { get; set; }
    public long? TotalMemoryMb { get; set; }
    public string? GpuModel { get; set; }
    public string? StorageType { get; set; }
    public bool IsVirtualMachine { get; set; }
    public string? Hypervisor { get; set; }
    public string? EnvironmentName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsActive { get; set; }
}
