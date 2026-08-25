using System.ComponentModel.DataAnnotations;

namespace DareToDance.Infrastructure.Options;

public sealed class GoogleAuthSettings
{
    public const string SectionName = "GoogleAuth";

    [Required]
    public string ClientId { get; init; } = string.Empty;
}
