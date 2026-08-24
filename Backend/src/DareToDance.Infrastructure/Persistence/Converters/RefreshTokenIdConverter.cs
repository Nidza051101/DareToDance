using DareToDance.Domain.RefreshToken.Id;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DareToDance.Infrastructure.Persistence.Converters;

public sealed class RefreshTokenIdConverter : ValueConverter<RefreshTokenId, Guid>
{
    public RefreshTokenIdConverter()
        : base(id => id.Value, value => RefreshTokenId.Create(value))
    {
    }
}
