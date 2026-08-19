using DareToDance.Domain.Common;

namespace DareToDance.Domain.Group.Id;

public sealed class GroupId : ValueObject
{
    public Guid Value { get; }

    private GroupId(Guid value)
    {
        Value = value;
    }

    public static GroupId CreateUnique()
    {
        return new GroupId(Guid.CreateVersion7());
    }

    public static GroupId Create(Guid value)
    {
        return new GroupId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
