using DareToDance.Application.Common.Services;

namespace DareToDance.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}