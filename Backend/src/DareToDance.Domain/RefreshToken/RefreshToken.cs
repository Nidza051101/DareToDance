using DareToDance.Domain.Common;
using DareToDance.Domain.RefreshToken.Id;
using DareToDance.Domain.User.Id;

namespace DareToDance.Domain.RefreshToken;

public sealed class RefreshToken : Entity<RefreshTokenId>
{
    public UserId UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    public RefreshTokenId? ReplacedByTokenId { get; private set; }

    private RefreshToken(
        RefreshTokenId id,
        UserId userId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static RefreshToken Create(
        UserId userId,
        string tokenHash,
        DateTime utcNow,
        DateTime expiresAtUtc)
    {
        return new RefreshToken(
            RefreshTokenId.CreateUnique(),
            userId,
            tokenHash,
            utcNow,
            expiresAtUtc);
    }

    public bool IsExpired(DateTime utcNow)
        => utcNow >= ExpiresAtUtc;

    public bool IsActive(DateTime utcNow)
        => RevokedAtUtc is null && !IsExpired(utcNow);

    public void Revoke(DateTime utcNow, RefreshTokenId? replacedByTokenId = null)
    {
        RevokedAtUtc = utcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
