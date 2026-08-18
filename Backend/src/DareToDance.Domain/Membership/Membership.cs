using DareToDance.Domain.Common;
using DareToDance.Domain.Membership.Id;
using DareToDance.Domain.User.Id;
using UserEntity = DareToDance.Domain.User.User;

namespace DareToDance.Domain.Membership;

public sealed class Membership : AggregateRoot<MembershipId>
{
    public UserId UserId { get; private set; }
    public UserEntity User { get; private set; } = null!;
    public string QrCode { get; private set; } //TODO: koliko dugo treba da bude validan par minuta ili na duzi period npr mesec dana. Aco: Traje jedan dan?
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public string Status { get; private set; }

    private Membership(
        MembershipId id,
        UserId userId,
        string qrCode,
        DateTime validFrom,
        DateTime validTo,
        string status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        UserId = userId;
        QrCode = qrCode;
        ValidFrom = validFrom;
        ValidTo = validTo;
        Status = status;
    }

    public static Membership Create(UserId userId, DateTime validFrom, DateTime validTo)
    {
        var utcNow = DateTime.UtcNow;

        return new Membership(
            MembershipId.CreateUnique(),
            userId,
            Guid.NewGuid().ToString(),
            validFrom,
            validTo,
            "active",
            utcNow,
            utcNow);
    }
}
