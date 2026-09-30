using Wallabee.Core.Configuration;
using Wallabee.Core.Models;
namespace Wallabee.Core.Heuristics;
public abstract class StaticRule(string name, int weight, string description) : IHeuristicRule
{
    public string Name => name;
    protected abstract bool Matches(FileContext context);
    public Task<ThreatIndicator?> EvaluateAsync(FileContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Matches(context) ? new ThreatIndicator("heuristics", name, description, weight) : null);
    }
}
public sealed class HighEntropyRule(HeuristicOptions options) : StaticRule("HighEntropy", 25, "High byte entropy in analyzed prefix")
{ protected override bool Matches(FileContext c) => c.Analysis is { BytesAnalyzed: >= 1024 } a && a.Entropy >= options.HighEntropyThreshold; }
public sealed class ExtensionMismatchRule() : StaticRule("ExtensionMismatch", 30, "File signature differs from extension")
{ protected override bool Matches(FileContext c) => c.Analysis?.DetectedExtension is { } ext && !c.Metadata.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase); }
public sealed class UnusualFileSizeRule(HeuristicOptions options) : StaticRule("UnusualFileSize", 10, "File size exceeds heuristic threshold")
{ protected override bool Matches(FileContext c) => c.Metadata.Length >= options.UnusualSizeBytes; }
public sealed class KnownHashRule(HeuristicOptions options) : StaticRule("KnownHash", 100, "Hash matches configured laboratory signature")
{ protected override bool Matches(FileContext c) => c.Hash is not null && options.KnownHashes.Contains(c.Hash.Value, StringComparer.OrdinalIgnoreCase); }
public sealed class MetadataMismatchRule() : StaticRule("MetadataMismatch", 15, "Metadata length is inconsistent with analysis")
{ protected override bool Matches(FileContext c) => c.Analysis?.BytesAnalyzed > c.Metadata.Length; }
public sealed class RepeatedPatternsRule() : StaticRule("RepeatedPatterns", 15, "Analyzed prefix is dominated by a repeated byte")
{ protected override bool Matches(FileContext c) => c.Analysis is { BytesAnalyzed: >= 1024, DominantByteRatio: > 0.95 }; }
public sealed class SyntheticAnomalyRule() : StaticRule("SyntheticAnomaly", 50, "Explicit inert laboratory anomaly filename marker")
{ protected override bool Matches(FileContext c) => c.Metadata.Name.Contains(".synthetic-anomaly.", StringComparison.OrdinalIgnoreCase); }
