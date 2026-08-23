using System.Security.Cryptography;
using System.Text;

namespace DareToDance.Infrastructure.Services;

// Plain SHA-256, deliberately WITHOUT the OTP pepper: a refresh secret carries
// 256 bits of entropy, so a database dump cannot be brute-forced offline the
// way a 6-digit code can — and skipping the pepper means rotating the OTP
// pepper never invalidates every signed-in session. The token id in the digest
// input binds the hash to its row, mirroring the OTP hasher's shape.
internal sealed class Sha256RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(Guid tokenId, string secret)
    {
        return Convert.ToBase64String(ComputeDigest(tokenId, secret));
    }

    public bool Verify(string storedHash, Guid tokenId, string secret)
    {
        Span<byte> stored = stackalloc byte[64];

        if (!Convert.TryFromBase64String(storedHash, stored, out var bytesWritten))
        {
            return false;
        }

        var computed = ComputeDigest(tokenId, secret);

        return CryptographicOperations.FixedTimeEquals(computed, stored[..bytesWritten]);
    }

    private static byte[] ComputeDigest(Guid tokenId, string secret)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes($"{tokenId}:{secret}"));
    }
}
