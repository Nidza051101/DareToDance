namespace DareToDance.Infrastructure.Services;

public interface IRefreshTokenHasher
{
    string Hash(Guid tokenId, string secret);

    bool Verify(string storedHash, Guid tokenId, string secret);
}
