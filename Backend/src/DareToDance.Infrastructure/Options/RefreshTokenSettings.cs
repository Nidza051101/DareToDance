using System.ComponentModel.DataAnnotations;

namespace DareToDance.Infrastructure.Options;

public sealed class RefreshTokenSettings
{
    public const string SectionName = "RefreshTokenSettings";

    // Each rotation grants this much lifetime again, so an actively used
    // session stays signed in.
    [Range(1, 90)]
    public int SlidingLifetimeDays { get; init; } = 30;

    // Hard cap counted from login: no amount of rotation extends a session
    // past this. Must be >= SlidingLifetimeDays (enforced at startup).
    [Range(1, 365)]
    public int AbsoluteLifetimeDays { get; init; } = 90;
}
