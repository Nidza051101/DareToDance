using System.Buffers.Text;
using System.Security.Cryptography;
using DareToDance.Domain.RefreshToken.Id;

namespace DareToDance.Infrastructure.Services;

// Wire shape: "{tokenId:N}.{base64url secret}" — a selector/verifier pair.
// The id half gives a primary-key lookup; only the secret half is sensitive,
// and only its hash is stored.
public static class RefreshTokenWireFormat
{
    private const int SecretByteLength = 32;

    // 32 hex chars + '.' + 43-char secret = 76; the cap only exists so the
    // validator can reject absurd payloads before the handler runs.
    public const int MaxWireLength = 128;

    public static string GenerateSecret()
    {
        return Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(SecretByteLength));
    }

    public static string Format(RefreshTokenId id, string secret)
    {
        return $"{id.Value:N}.{secret}";
    }

    public static bool TryParse(string wireToken, out Guid tokenId, out string secret)
    {
        tokenId = default;
        secret = string.Empty;

        if (string.IsNullOrEmpty(wireToken) || wireToken.Length > MaxWireLength)
        {
            return false;
        }

        var parts = wireToken.Split('.');

        if (parts.Length != 2 || parts[1].Length == 0)
        {
            return false;
        }

        if (!Guid.TryParseExact(parts[0], "N", out tokenId))
        {
            return false;
        }

        secret = parts[1];
        return true;
    }
}
