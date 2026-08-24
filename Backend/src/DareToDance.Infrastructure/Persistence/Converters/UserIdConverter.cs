using DareToDance.Domain.User.Id;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DareToDance.Infrastructure.Persistence.Converters;

public sealed class UserIdConverter : ValueConverter<UserId, Guid>
{
    public UserIdConverter()
        : base(id => id.Value, value => UserId.Create(value))
    {
    }
}
