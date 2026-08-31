using System.ComponentModel.DataAnnotations;

namespace NotificationService.Infrastructure.Options;

public sealed class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    [Required]
    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 5672;

    public string VirtualHost { get; init; } = "/";

    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
