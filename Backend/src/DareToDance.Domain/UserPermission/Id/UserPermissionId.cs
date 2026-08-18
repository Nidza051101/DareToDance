using DareToDance.Domain.Common;

namespace DareToDance.Domain.UserPermission.Id;

public sealed class UserPermissionId : ValueObject
{
    public Guid Value { get; }

    private UserPermissionId(Guid value)
    {
        Value = value;
    }

    public static UserPermissionId CreateUnique()
    {
        return new UserPermissionId(Guid.CreateVersion7());
    }

    public static UserPermissionId Create(Guid value)
    {
        return new UserPermissionId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
