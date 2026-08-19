namespace DareToDance.Infrastructure.Options;

public sealed class OtpSettings
{
    public const string SectionName = "OtpSettings";

    public int CodeLength { get; set; }
    public int ExpiryMinutes { get; set; }
    public int MaxFailedAttempts { get; set; }
    public int ResendCooldownSeconds { get; set; }
}
