using DareToDance.Domain.User;

namespace DareToDance.Infrastructure.Services;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
}

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc)
{
    public override string ToString()
        => $"AccessToken {{ Token = [REDACTED], ExpiresAtUtc = {ExpiresAtUtc:O} }}";
}
