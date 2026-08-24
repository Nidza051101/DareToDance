using DareToDance.Domain.Common;
using DareToDance.Domain.RefreshToken.Id;
using DareToDance.Domain.User.Id;
using ErrorOr;

namespace DareToDance.Domain.RefreshToken;

public sealed class RefreshToken : AggregateRoot<RefreshTokenId>
{
    public UserId UserId { get; private set; }

    // Groups every rotation of one login into a session: reuse of any consumed
    // member is treated as theft and revokes the whole family.
    public Guid FamilyId { get; private set; }

    public string TokenHash { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    // Hard cap stamped at login and copied to every successor, so a session
    // can slide with use but never outlive the family's absolute lifetime.
    public DateTime AbsoluteExpiresAtUtc { get; private set; }

    public DateTime? ConsumedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    // Audit pointer only — deliberately not a foreign key, so purging old
    // rows never has to untangle a self-referencing chain.
    public RefreshTokenId? ReplacedById { get; private set; }

    private RefreshToken(
        RefreshTokenId id,
        UserId userId,
        Guid familyId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime absoluteExpiresAtUtc,
        DateTime? consumedAtUtc,
        DateTime? revokedAtUtc,
        RefreshTokenId? replacedById,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        UserId = userId;
        FamilyId = familyId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        AbsoluteExpiresAtUtc = absoluteExpiresAtUtc;
        ConsumedAtUtc = consumedAtUtc;
        RevokedAtUtc = revokedAtUtc;
        ReplacedById = replacedById;
    }

    public static ErrorOr<RefreshToken> Create(
        RefreshTokenId id,
        UserId userId,
        Guid familyId,
        string tokenHash,
        DateTime utcNow,
        TimeSpan slidingLifetime,
        DateTime absoluteExpiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return RefreshTokenErrors.TokenHashRequired;
        }

        if (slidingLifetime <= TimeSpan.Zero)
        {
            return RefreshTokenErrors.SlidingLifetimeNotPositive;
        }

        if (absoluteExpiresAtUtc <= utcNow)
        {
            return RefreshTokenErrors.AbsoluteExpiryNotInFuture;
        }

        var slidingExpiresAtUtc = utcNow + slidingLifetime;

        return new RefreshToken(
            id,
            userId,
            familyId,
            tokenHash,
            slidingExpiresAtUtc < absoluteExpiresAtUtc ? slidingExpiresAtUtc : absoluteExpiresAtUtc,
            absoluteExpiresAtUtc,
            consumedAtUtc: null,
            revokedAtUtc: null,
            replacedById: null,
            utcNow,
            utcNow);
    }

    public ErrorOr<Success> Consume(DateTime utcNow, RefreshTokenId replacedById)
    {
        if (ConsumedAtUtc is not null || RevokedAtUtc is not null)
        {
            return RefreshTokenErrors.AlreadyFinalized;
        }

        if (utcNow >= ExpiresAtUtc)
        {
            return RefreshTokenErrors.Expired;
        }

        ConsumedAtUtc = utcNow;
        ReplacedById = replacedById;
        return Result.Success;
    }
}
