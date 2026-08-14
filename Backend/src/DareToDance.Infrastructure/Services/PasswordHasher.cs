using Microsoft.AspNetCore.Identity;

namespace DareToDance.Infrastructure.Services;

internal sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string passwordHash, string providedPassword)
    {
        return _hasher.VerifyHashedPassword(null!, passwordHash, providedPassword)
               is not PasswordVerificationResult.Failed;
    }
}
