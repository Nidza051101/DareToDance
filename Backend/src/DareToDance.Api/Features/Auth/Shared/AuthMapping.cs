namespace DareToDance.Api.Features.Auth.Shared;

public static class AuthMapping
{
    public static AccessTokenResponse ToResponse(this LoginResult loginResult)
    {
        return new AccessTokenResponse(loginResult.AccessToken, loginResult.ExpiresAtUtc);
    }
}
