using DareToDance.Domain.Common;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.LoginCode.Id;

namespace DareToDance.Domain.LoginCode;

public sealed class LoginCode : AggregateRoot<LoginCodeId>
{
    public UserId UserId { get; private set; }
    public LoginChannel Channel { get; private set; }
    public string CodeHash { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? ConsumedAtUtc { get; private set; }
    public int FailedAttempts { get; private set; }

    private LoginCode(
        LoginCodeId id,
        UserId userId,
        LoginChannel channel,
        string codeHash,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        UserId = userId;
        Channel = channel;
        CodeHash = codeHash;
        ExpiresAtUtc = expiresAtUtc;
        FailedAttempts = 0;
    }

    public static LoginCode Create(UserId userId, LoginChannel channel, string codeHash, DateTime expiresAtUtc)
    {
        var utcNow = DateTime.UtcNow;

        return new LoginCode(
            LoginCodeId.CreateUnique(),
            userId,
            channel,
            codeHash,
            expiresAtUtc,
            utcNow,
            utcNow);
    }

    public bool IsConsumed => ConsumedAtUtc is not null;

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

    public void MarkConsumed(DateTime utcNow)
    {
        ConsumedAtUtc = utcNow;
        MarkAsUpdated(utcNow);
    }

    public void RegisterFailedAttempt(DateTime utcNow)
    {
        FailedAttempts++;
        MarkAsUpdated(utcNow);
    }
}
