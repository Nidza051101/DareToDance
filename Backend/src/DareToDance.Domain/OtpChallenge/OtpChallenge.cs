using DareToDance.Domain.Common;
using DareToDance.Domain.OtpChallenge.Id;
using DareToDance.Domain.User.Id;
using ErrorOr;

namespace DareToDance.Domain.OtpChallenge;

public sealed class OtpChallenge : AggregateRoot<OtpChallengeId>
{
    public UserId UserId { get; private set; }
    public string CodeHash { get; private set; }
    public OtpPurpose Purpose { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public int FailedAttempts { get; private set; }
    public DateTime? ConsumedAtUtc { get; private set; }
    public DateTime? InvalidatedAtUtc { get; private set; }

    private OtpChallenge(
        OtpChallengeId id,
        UserId userId,
        string codeHash,
        OtpPurpose purpose,
        DateTime expiresAtUtc,
        int failedAttempts,
        DateTime? consumedAtUtc,
        DateTime? invalidatedAtUtc,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        UserId = userId;
        CodeHash = codeHash;
        Purpose = purpose;
        ExpiresAtUtc = expiresAtUtc;
        FailedAttempts = failedAttempts;
        ConsumedAtUtc = consumedAtUtc;
        InvalidatedAtUtc = invalidatedAtUtc;
    }
    
    public static ErrorOr<OtpChallenge> Create(
        OtpChallengeId id,
        UserId userId,
        string codeHash,
        OtpPurpose purpose,
        DateTime utcNow,
        TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(codeHash))
        {
            return OtpChallengeErrors.CodeHashRequired;
        }

        if (lifetime <= TimeSpan.Zero)
        {
            return OtpChallengeErrors.LifetimeNotPositive;
        }

        return new OtpChallenge(
            id,
            userId,
            codeHash,
            purpose,
            utcNow + lifetime,
            failedAttempts: 0,
            consumedAtUtc: null,
            invalidatedAtUtc: null,
            utcNow,
            utcNow);
    }

    public bool IsActive(DateTime utcNow, int maxFailedAttempts)
    {
        return ConsumedAtUtc is null
               && InvalidatedAtUtc is null
               && utcNow < ExpiresAtUtc
               && FailedAttempts < maxFailedAttempts;
    }

    public ErrorOr<Success> RegisterFailedAttempt()
    {
        if (ConsumedAtUtc is not null || InvalidatedAtUtc is not null)
        {
            return OtpChallengeErrors.AlreadyFinalized;
        }

        FailedAttempts++;
        return Result.Success;
    }

    public ErrorOr<Success> Consume(DateTime utcNow)
    {
        if (ConsumedAtUtc is not null || InvalidatedAtUtc is not null)
        {
            return OtpChallengeErrors.AlreadyFinalized;
        }

        if (utcNow >= ExpiresAtUtc)
        {
            return OtpChallengeErrors.Expired;
        }

        ConsumedAtUtc = utcNow;
        return Result.Success;
    }

    public void Invalidate(DateTime utcNow)
    {
        if (ConsumedAtUtc is not null || InvalidatedAtUtc is not null)
        {
            return;
        }

        InvalidatedAtUtc = utcNow;
    }
}
