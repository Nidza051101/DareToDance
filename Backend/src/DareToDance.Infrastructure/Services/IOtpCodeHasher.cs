namespace DareToDance.Infrastructure.Services;

public interface IOtpCodeHasher
{
    string Hash(Guid challengeId, string code);

    bool Verify(string storedHash, Guid challengeId, string code);
}
