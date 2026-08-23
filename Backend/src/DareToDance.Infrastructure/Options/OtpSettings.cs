using System.ComponentModel.DataAnnotations;

namespace DareToDance.Infrastructure.Options;

public sealed class OtpSettings
{
    public const string SectionName = "OtpSettings";

    [Range(6, 9)]
    public int CodeLength { get; init; } = 6;

    [Range(30, 900)]
    public int ExpirySeconds { get; init; } = 300;

    [Range(3, 10)]
    public int MaxFailedAttempts { get; init; } = 3;

    [Range(30, 600)]
    public int ResendCooldownSeconds { get; init; } = 60;

    [Range(1, 100)]
    public int MaxCodesPerDay { get; init; } = 10;

    // Server-side HMAC key for hashing OTP codes at rest. Must never live in
    // the database or appsettings — user-secrets in Development, env var otherwise.
    [Required, MinLength(32)]
    public string Pepper { get; init; } = string.Empty;
}
