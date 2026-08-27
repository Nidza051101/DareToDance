using System.ComponentModel.DataAnnotations;

namespace NotificationService.Infrastructure.Options;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    [Required]
    public string GmailAddress { get; init; } = string.Empty;

    // App Password — 16 karaktera, NE prava lozinka naloga. Prazno u
    // appsettings.json; prava vrednost stiže preko env varijable
    // EmailSettings__AppPassword (lokalno: .env, produkcija: GitHub Actions
    // Secrets → deploy korak). V. artifact "Notification gRPC Flow", deo 4.
    [Required]
    public string AppPassword { get; init; } = string.Empty;

    public string SmtpHost { get; init; } = "smtp.gmail.com";

    public int SmtpPort { get; init; } = 587;
}
