using System.Security.Cryptography;
using System.Text;
using DareToDance.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DareToDance.Infrastructure.Services;

internal sealed class RefreshTokenService(IOptions<RefreshTokenSettings> refreshTokenOptions) : IRefreshTokenService
{
    private const int RawTokenSizeInBytes = 64;

    public (string RawToken, string TokenHash, DateTime ExpiresAtUtc) Generate(DateTime utcNow)
    {
        var rawToken = GenerateRawToken();
        var tokenHash = Hash(rawToken);
        var expiresAtUtc = utcNow.AddDays(refreshTokenOptions.Value.ExpiryDays);

        return (rawToken, tokenHash, expiresAtUtc);
    }
    
    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));

        return Convert.ToHexString(bytes);
    }

    private static string GenerateRawToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(RawTokenSizeInBytes);

        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
