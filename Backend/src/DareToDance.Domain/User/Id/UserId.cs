using DareToDance.Domain.Common;

namespace DareToDance.Domain.User.Id;

public sealed class UserId : ValueObject
{
    public Guid Value { get; }

    private UserId(Guid value)
    {
        Value = value;
    }

    public static UserId CreateUnique()
    {
        return new UserId(Guid.CreateVersion7());
    }

    public static UserId Create(Guid value)
    {
        return new UserId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
