using Wallabee.Core.Models;

namespace Wallabee.Core.Interfaces;

public interface IFileScanner
{
    Task<FileScanResult> ScanFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<FileScanResult>> ScanDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);
}

public interface IHashCalculator
{
    Task<HashResult> CalculateAsync(Stream content, CancellationToken cancellationToken = default);
}

public interface IHeuristicAnalyzer
{
    Task<IReadOnlyCollection<ThreatIndicator>> AnalyzeAsync(FileContext context, CancellationToken cancellationToken = default);
}

public interface IRiskScorer
{
    RiskAssessment Assess(IEnumerable<ThreatIndicator> indicators);
}

public interface IThreatDetector
{
    Task<DetectionResult> DetectAsync(FileContext context, CancellationToken cancellationToken = default);
}

public interface IQuarantineService
{
    Task<QuarantineEntry> QuarantineAsync(string filePath, CancellationToken cancellationToken = default);
    Task RestoreAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<QuarantineEntry>> ListAsync(CancellationToken cancellationToken = default);
}