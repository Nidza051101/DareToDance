using System.ComponentModel.DataAnnotations;

namespace DareToDance.Application.Services.Authentication.Otp;

public class OtpSettings
{
    public const string SectionName = "OtpSettings";

    [Range(4, 10)]
    public int CodeLength { get; init; }

    [Range(1, 60)]
    public int ExpiryMinutes { get; init; }

    [Range(1, 10)]
    public int MaxFailedAttempts { get; init; }

    [Range(0, 3600)]
    public int ResendCooldownSeconds { get; init; }
}
