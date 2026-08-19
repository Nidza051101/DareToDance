using DareToDance.Domain.Common;

namespace DareToDance.Domain.PermissionEntity.Id;

public sealed class PermissionId : ValueObject
{
    public Guid Value { get; }

    private PermissionId(Guid value)
    {
        Value = value;
    }

    public static PermissionId CreateUnique()
    {
        return new PermissionId(Guid.CreateVersion7());
    }

    public static PermissionId Create(Guid value)
    {
        return new PermissionId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
