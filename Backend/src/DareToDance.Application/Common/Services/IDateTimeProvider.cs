namespace DareToDance.Application.Common.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}