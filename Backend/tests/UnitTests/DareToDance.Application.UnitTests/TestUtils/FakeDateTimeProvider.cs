using DareToDance.Application.Common.Services;

namespace DareToDance.Application.UnitTests.TestUtils;

public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
}
