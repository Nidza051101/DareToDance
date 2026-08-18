using DareToDance.Domain.Common;

namespace DareToDance.Domain.Membership.Id;

public sealed class MembershipId : ValueObject
{
    public Guid Value { get; }

    private MembershipId(Guid value)
    {
        Value = value;
    }

    public static MembershipId CreateUnique()
    {
        return new MembershipId(Guid.CreateVersion7());
    }

    public static MembershipId Create(Guid value)
    {
        return new MembershipId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
