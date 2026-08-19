namespace DareToDance.Api.Features.Auth.Shared;

public sealed record AccessTokenResponse(string AccessToken, DateTime ExpiresAtUtc);
