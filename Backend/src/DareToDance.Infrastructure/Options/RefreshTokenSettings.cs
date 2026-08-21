namespace DareToDance.Infrastructure.Options;

public sealed class RefreshTokenSettings
{
    public const string SectionName = "RefreshTokenSettings";

    public int ExpiryDays { get; set; }
}
