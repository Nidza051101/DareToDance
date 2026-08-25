using Google.Apis.Auth;

namespace DareToDance.Infrastructure.Services;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    public async Task<GoogleTokenPayload> ValidateAsync(string idToken, string clientId)
    {
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { clientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

        return new GoogleTokenPayload(payload.Email);
    }
}