using DareToDance.Domain.Common;

namespace DareToDance.Domain.DanceStyle.Id;

public sealed class DanceStyleId : ValueObject
{
    public Guid Value { get; }

    private DanceStyleId(Guid value)
    {
        Value = value;
    }

    public static DanceStyleId CreateUnique()
    {
        return new DanceStyleId(Guid.CreateVersion7());
    }

    public static DanceStyleId Create(Guid value)
    {
        return new DanceStyleId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}