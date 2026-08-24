using System.ComponentModel.DataAnnotations;

namespace DareToDance.Infrastructure.Options;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    // Symmetric HS256 signing key. Must never live in appsettings —
    // user-secrets in Development, env var otherwise.
    [Required, MinLength(32)]
    public string Secret { get; init; } = string.Empty;

    [Range(5, 120)]
    public int ExpiryMinutes { get; init; } = 15;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;
}
