using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wallabee.Application.Scanning.Services;
using Wallabee.Core.Configuration;
using Wallabee.Core.Heuristics;
using Wallabee.Core.Scoring;
using Wallabee.Infrastructure.FileSystem;
using Wallabee.Infrastructure.Hashing;

namespace Wallabee.Infrastructure.Tests;

public sealed class FileSystemTests
{
    [Fact]
    public async Task CalculateAsync_ReturnsKnownSha256()
    {
        await using var content = new MemoryStream("abc"u8.ToArray());
        var result = await new Sha256HashCalculator().CalculateAsync(content);

        Assert.Equal("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", result.Value);
    }

    [Fact]
    public void IsInsideLabDirectory_RejectsTraversalOutsideLab()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var validator = new LabPathValidator(root);

        Assert.True(validator.IsInsideLabDirectory(Path.Combine(root, "samples", "safe.txt")));
        Assert.False(validator.IsInsideLabDirectory(Path.Combine(root, "..", "outside.txt")));
    }

    [Fact]
    public async Task ScanFileAsync_ScansBenignLaboratoryFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var filePath = Path.Combine(root, "sample.txt");
        await File.WriteAllTextAsync(filePath, "benign sample");
        try
        {
            var pipeline = new DetectionPipeline(new HeuristicEngine([]), new IndicatorRiskScorer());
            var scanner = new LocalFileSystemScanner(new Sha256HashCalculator(), pipeline, new LabPathValidator(root), Options.Create(new WallabeeOptions { LabRoot = root }), NullLogger<LocalFileSystemScanner>.Instance);

            var result = await scanner.ScanFileAsync(filePath);

            Assert.Equal(0, result.RiskScore);
            Assert.NotNull(result.Sha256);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}