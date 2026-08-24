using DareToDance.Domain.Common;

namespace DareToDance.Domain.OtpChallenge.Id;

public sealed class OtpChallengeId : ValueObject
{
    public Guid Value { get; }

    private OtpChallengeId(Guid value)
    {
        Value = value;
    }

    public static OtpChallengeId CreateUnique()
    {
        return new OtpChallengeId(Guid.CreateVersion7());
    }

    public static OtpChallengeId Create(Guid value)
    {
        return new OtpChallengeId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
