using DareToDance.Domain.Common;
using DareToDance.Domain.Membership.Id;
using DareToDance.Domain.User.Id;

namespace DareToDance.Domain.Membership;

public sealed class Membership : AggregateRoot<MembershipId>
{
    public UserId UserId { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public MembershipStatus Status { get; private set; }

    private Membership(
        MembershipId id,
        UserId userId,
        DateTime validFrom,
        DateTime validTo,
        MembershipStatus status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        UserId = userId;
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
            validFrom,
            validTo,
            MembershipStatus.Active,
            utcNow,
            utcNow);
    }
}
