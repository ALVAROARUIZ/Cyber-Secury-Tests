using Wallabee.Core.Enums;
namespace Wallabee.Infrastructure.Persistence.Entities;
public sealed class SystemMetricEntity
{
    public long SystemMetricSampleId { get; set; }
    public Guid? ExperimentId { get; set; }
    public Guid? RoundId { get; set; }
    public Guid? BenchmarkId { get; set; }
    public int? TestHostId { get; set; }
    public DateTime TimestampUtc { get; set; }
    public decimal? CpuPercent { get; set; }
    public decimal? MemoryUsedMb { get; set; }
    public decimal? MemoryPercent { get; set; }
    public long? DiskReadBytesPerSecond { get; set; }
    public long? DiskWriteBytesPerSecond { get; set; }
    public decimal? DiskReadOperationsPerSecond { get; set; }
    public decimal? DiskWriteOperationsPerSecond { get; set; }
    public decimal? ProcessCpuPercent { get; set; }
    public decimal? ProcessMemoryMb { get; set; }
    public int? ThreadCount { get; set; }
    public int? HandleCount { get; set; }
    public decimal? CpuPeakPercent { get; set; }
    public decimal? MemoryPeakMb { get; set; }
}
