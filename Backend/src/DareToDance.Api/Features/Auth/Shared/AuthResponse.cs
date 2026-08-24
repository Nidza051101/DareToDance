namespace DareToDance.Api.Features.Auth.Shared;

public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
