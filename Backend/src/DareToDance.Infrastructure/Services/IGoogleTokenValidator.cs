namespace DareToDance.Infrastructure.Services;

public interface IGoogleTokenValidator
{
    Task<GoogleTokenPayload> ValidateAsync(string idToken, string clientId);
}

public sealed record GoogleTokenPayload(string Email);