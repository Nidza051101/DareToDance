using DareToDance.Domain.OtpChallenge.Id;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DareToDance.Infrastructure.Persistence.Converters;

public sealed class OtpChallengeIdConverter : ValueConverter<OtpChallengeId, Guid>
{
    public OtpChallengeIdConverter()
        : base(id => id.Value, value => OtpChallengeId.Create(value))
    {
    }
}
