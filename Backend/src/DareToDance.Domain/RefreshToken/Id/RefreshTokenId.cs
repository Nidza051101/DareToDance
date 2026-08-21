using DareToDance.Domain.Common;

namespace DareToDance.Domain.RefreshToken.Id;

public sealed class RefreshTokenId : ValueObject
{
    public Guid Value { get; }

    private RefreshTokenId(Guid value)
    {
        Value = value;
    }

    public static RefreshTokenId CreateUnique()
    {
        return new RefreshTokenId(Guid.CreateVersion7());
    }

    public static RefreshTokenId Create(Guid value)
    {
        return new RefreshTokenId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
