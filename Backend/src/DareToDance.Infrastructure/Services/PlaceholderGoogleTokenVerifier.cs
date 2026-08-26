using System.Text;
using System.Text.Json;
using ErrorOr;

namespace DareToDance.Infrastructure.Services;

internal sealed class PlaceholderGoogleTokenVerifier : IGoogleTokenVerifier
{
    public Task<ErrorOr<GoogleIdentity>> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        var parts = idToken.Split('.');

        if (parts.Length != 3)
        {
            return Task.FromResult<ErrorOr<GoogleIdentity>>(GoogleAuthErrors.InvalidToken);
        }

        try
        {
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var payload = JsonDocument.Parse(payloadJson);
            var root = payload.RootElement;

            var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
            var firstName = root.TryGetProperty("given_name", out var givenProp) ? givenProp.GetString() : null;
            var lastName = root.TryGetProperty("family_name", out var familyProp) ? familyProp.GetString() : null;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Task.FromResult<ErrorOr<GoogleIdentity>>(GoogleAuthErrors.InvalidToken);
            }

            return Task.FromResult<ErrorOr<GoogleIdentity>>(
                new GoogleIdentity(email, firstName ?? string.Empty, lastName ?? string.Empty));
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return Task.FromResult<ErrorOr<GoogleIdentity>>(GoogleAuthErrors.InvalidToken);
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }
}
