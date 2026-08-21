namespace DareToDance.Api.Features.Auth.Shared;

public sealed record AuthTokensResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
