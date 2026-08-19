using DareToDance.Domain.User;

namespace DareToDance.Infrastructure.Services;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAtUtc) GenerateToken(User user);
}
