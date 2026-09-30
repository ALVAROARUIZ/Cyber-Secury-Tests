using Wallabee.Core.Models;
namespace Wallabee.Infrastructure.FileSystem;
public static class StaticFileAnalyzer
{
    // Bounded prefix; bytes never leave this adapter or enter persistence.
    public static async Task<StaticAnalysis> AnalyzeAsync(Stream stream, CancellationToken ct)
    {
        stream.Position = 0;
        var buffer = new byte[65536];
        var read = await stream.ReadAtLeastAsync(buffer, buffer.Length, false, ct);
        var counts = new int[256];
        for (var i = 0; i < read; i++) counts[buffer[i]]++;
        var entropy = 0d;
        foreach (var count in counts.Where(c => c > 0)) { var p = count / (double)read; entropy -= p * Math.Log2(p); }
        string? extension = read >= 2 && buffer[0] == 0x4d && buffer[1] == 0x5a ? ".exe" :
            read >= 4 && buffer.AsSpan(0, 4).SequenceEqual("%PDF"u8) ? ".pdf" :
            read >= 8 && buffer.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}) ? ".png" : null;
        return new(entropy, read == 0 ? 0 : counts.Max() / (double)read, extension, read);
    }
}
