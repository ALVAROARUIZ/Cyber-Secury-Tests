using System.Security.Cryptography;
using Wallabee.Core.Interfaces;
using Wallabee.Core.Models;

namespace Wallabee.Infrastructure.Hashing;

public sealed class Sha256HashCalculator : IHashCalculator
{
    public async Task<HashResult> CalculateAsync(Stream content, CancellationToken cancellationToken = default)
    {
        var hash = await SHA256.HashDataAsync(content, cancellationToken).ConfigureAwait(false);
        return new HashResult("SHA-256", Convert.ToHexString(hash));
    }
}