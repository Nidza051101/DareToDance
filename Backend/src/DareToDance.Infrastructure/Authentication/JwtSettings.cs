using System.ComponentModel.DataAnnotations;

namespace DareToDance.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required]
    [MinLength(32, ErrorMessage = "JwtSettings:Secret must be at least 32 characters (256 bits) for HMAC-SHA256.")]
    public string Secret { get; init; } = null!;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; init; }

    [Required]
    public string Issuer { get; init; } = null!;

    [Required]
    public string Audience { get; init; } = null!;
}
