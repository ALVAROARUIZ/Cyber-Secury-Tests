using Microsoft.Extensions.Logging.Abstractions;
using Wallabee.Application.Scanning.Services;
using Wallabee.Core.Enums;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;
using Wallabee.Core.Policies;
namespace Wallabee.Application.Tests;
public sealed class ScanFlowTests
{
    [Fact]
    public async Task ScanPersistsCorrelationAndSanitizedTelemetryBeforeResponse()
    {
        var repository = new Repository(); var scanner = new Scanner(); var quarantine = new Quarantine();
        var service = new ScanFileService(scanner, repository, new PolicyEngine(), quarantine, NullLogger<ScanFileService>.Instance);
        var result = await service.ExecuteAsync("local", new ExperimentContext("E", "R", "S", "HB"));
        Assert.Equal("S", repository.Result!.SampleId); Assert.Equal("HB", result.HoneyBadgerVersion);
        Assert.DoesNotContain("private", repository.Event!.Payload); Assert.DoesNotContain("filePath", repository.Event.Payload);
        Assert.Equal(result.ScanId, Assert.Single(result.Indicators).ScanId); Assert.False(quarantine.Called);
    }
    [Fact]
    public async Task FailedPersistenceNeverQuarantinesAndCancellationPropagates()
    {
        var quarantine = new Quarantine(); var repository = new Repository { Fail = true };
        var service = new ScanFileService(new Scanner(), repository, new PolicyEngine(true), quarantine, NullLogger<ScanFileService>.Instance);
        await Assert.ThrowsAsync<IOException>(() => service.ExecuteAsync("local")); Assert.False(quarantine.Called);
        using var stop = new CancellationTokenSource(); stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ExecuteAsync("local", stop.Token));
    }
    private sealed class Scanner : IFileScanner
    {
        public Task<FileScanResult> ScanFileAsync(string path, CancellationToken cancellationToken = default)
        { cancellationToken.ThrowIfCancellationRequested(); return Task.FromResult(new FileScanResult("/private/sample.bin", 1, "ABC", 100, DetectionLevel.Critical, [new("heuristics", "KnownHash", "configured match", 100)], TimeSpan.FromMilliseconds(1))); }
        public Task<IReadOnlyCollection<FileScanResult>> ScanDirectoryAsync(string path, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
    private sealed class Repository : IScanResultRepository
    {
        public bool Fail; public FileScanResult? Result; public TelemetryEvent? Event;
        public Task SaveAsync(FileScanResult r, TelemetryEvent e, CancellationToken ct = default) { if(Fail) throw new IOException(); Result = r; Event = e; return Task.CompletedTask; }
        public Task<FileScanResult?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Result);
        public Task<IReadOnlyList<FileScanResult>> FindAsync(ScanFilter filter, CancellationToken ct = default) => throw new NotSupportedException();
    }
    private sealed class Quarantine : IQuarantineService
    {
        public bool Called;
        public Task<QuarantineEntry> QuarantineAsync(string path, CancellationToken cancellationToken = default) { Called = true; throw new NotSupportedException(); }
        public Task RestoreAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<QuarantineEntry>> ListAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
