using DareToDance.Domain.Common;
using DareToDance.Domain.OtpChallenge.Id;
using DareToDance.Domain.User.Id;

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
    
    public static OtpChallenge Create(
        OtpChallengeId id,
        UserId userId,
        string codeHash,
        OtpPurpose purpose,
        DateTime utcNow,
        TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(codeHash))
        {
            throw new ArgumentException("The code hash is required.", nameof(codeHash));
        }

        if (lifetime <= TimeSpan.Zero)
        {
            throw new ArgumentException("The lifetime must be positive.", nameof(lifetime));
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

    public void RegisterFailedAttempt()
    {
        if (ConsumedAtUtc is not null || InvalidatedAtUtc is not null)
        {
            throw new InvalidOperationException(
                "Cannot register a failed attempt on a consumed or invalidated challenge.");
        }

        FailedAttempts++;
    }

    public void Consume(DateTime utcNow)
    {
        if (ConsumedAtUtc is not null || InvalidatedAtUtc is not null)
        {
            throw new InvalidOperationException(
                "The challenge has already been consumed or invalidated.");
        }

        if (utcNow >= ExpiresAtUtc)
        {
            throw new InvalidOperationException("Cannot consume an expired challenge.");
        }

        ConsumedAtUtc = utcNow;
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
