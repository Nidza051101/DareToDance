using System.Security.Cryptography;
using System.Text;
using DareToDance.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DareToDance.Infrastructure.Services;

// HMAC-SHA256 keyed by a server-side pepper that never touches the database:
// a 6-digit code has only 10^6 possible values, so any unkeyed hash of it can
// be brute-forced offline from a database dump. The challenge id in the MAC
// input acts as a per-row salt so equal codes never share a hash.
internal sealed class HmacOtpCodeHasher(IOptions<OtpSettings> otpOptions) : IOtpCodeHasher
{
    public string Hash(Guid challengeId, string code)
    {
        return Convert.ToBase64String(ComputeMac(challengeId, code));
    }

    public bool Verify(string storedHash, Guid challengeId, string code)
    {
        Span<byte> stored = stackalloc byte[64];

        if (!Convert.TryFromBase64String(storedHash, stored, out var bytesWritten))
        {
            return false;
        }

        var computed = ComputeMac(challengeId, code);

        return CryptographicOperations.FixedTimeEquals(computed, stored[..bytesWritten]);
    }

    private byte[] ComputeMac(Guid challengeId, string code)
    {
        var key = Encoding.UTF8.GetBytes(otpOptions.Value.Pepper);
        var payload = Encoding.UTF8.GetBytes($"{challengeId}:{code}");

        return HMACSHA256.HashData(key, payload);
    }
}
