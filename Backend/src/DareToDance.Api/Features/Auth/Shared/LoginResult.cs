using DareToDance.Domain.User;

namespace DareToDance.Api.Features.Auth.Shared;

public sealed record LoginResult(User User, string AccessToken, DateTime ExpiresAtUtc);
