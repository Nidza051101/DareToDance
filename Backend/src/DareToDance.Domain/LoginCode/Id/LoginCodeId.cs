using DareToDance.Domain.Common;

namespace DareToDance.Domain.LoginCode.Id;

public sealed class LoginCodeId : ValueObject
{
    public Guid Value { get; }

    private LoginCodeId(Guid value)
    {
        Value = value;
    }

    public static LoginCodeId CreateUnique()
    {
        return new LoginCodeId(Guid.CreateVersion7());
    }

    public static LoginCodeId Create(Guid value)
    {
        return new LoginCodeId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
