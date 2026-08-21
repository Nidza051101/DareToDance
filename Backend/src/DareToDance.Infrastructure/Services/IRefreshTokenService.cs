namespace DareToDance.Infrastructure.Services;

public interface IRefreshTokenService
{
    (string RawToken, string TokenHash, DateTime ExpiresAtUtc) Generate(DateTime utcNow);

    string Hash(string rawToken);
}
