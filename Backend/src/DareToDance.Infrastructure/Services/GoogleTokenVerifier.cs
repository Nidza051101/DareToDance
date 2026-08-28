using DareToDance.Infrastructure.Options;
using ErrorOr;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace DareToDance.Infrastructure.Services;

// Prava implementacija IGoogleTokenVerifier-a za sve van Development-a.
// PlaceholderGoogleTokenVerifier samo dekodira JWT payload BEZ provere
// potpisa — ovde Google.Apis.Auth proverava potpis, issuer i audience
// (GoogleAuth:ClientId), isti paket koji već koristi GoogleTokenValidator.
internal sealed class GoogleTokenVerifier(IOptions<GoogleAuthSettings> options) : IGoogleTokenVerifier
{
    public async Task<ErrorOr<GoogleIdentity>> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idToken) || idToken.Split('.').Length != 3)
        {
            return GoogleAuthErrors.InvalidToken;
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { options.Value.ClientId },
                });

            return new GoogleIdentity(
                payload.Email,
                payload.GivenName ?? string.Empty,
                payload.FamilyName ?? string.Empty);
        }
        // InvalidJwtException — loš potpis / audience / istekao. Ostali tipovi —
        // strukturno neispravan token (loš base64, nije JSON). Sve je "nevažeći
        // token" za pozivaoca; mrežne greške pri dohvatu Google sertifikata se
        // NAMERNO ne hvataju ovde (to je 500 koji vredi ponoviti).
        catch (Exception ex) when (ex is InvalidJwtException
            or Newtonsoft.Json.JsonException
            or FormatException
            or ArgumentException)
        {
            return GoogleAuthErrors.InvalidToken;
        }
    }
}
