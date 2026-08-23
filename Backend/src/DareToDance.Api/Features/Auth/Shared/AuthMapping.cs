namespace DareToDance.Api.Features.Auth.Shared;

public static class AuthMapping
{
    public static AuthResponse ToResponse(this AuthResult result)
        => new(
            result.AccessToken.Token,
            "Bearer",
            result.AccessToken.ExpiresAtUtc,
            result.User.Id.Value,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc);
}
