using System.ComponentModel.DataAnnotations;

namespace DareToDance.Infrastructure.Options;

public sealed class NotificationServiceSettings
{
    public const string SectionName = "NotificationService";

    // Lokalno/produkcija: ime servisa iz docker-compose.yml (Docker DNS ga
    // sam razreši) — nikad javno izložena adresa. V. artifact
    // "Notification gRPC Flow", deo 4.
    [Required]
    public string GrpcAddress { get; init; } = string.Empty;
}
