using System.ComponentModel.DataAnnotations;

namespace NotificationService.Infrastructure.Options;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    [Required]
    public string GmailAddress { get; init; } = string.Empty;

    [Required]
    public string AppPassword { get; init; } = string.Empty;

    public string SmtpHost { get; init; } = "smtp.gmail.com";

    public int SmtpPort { get; init; } = 587;
}
