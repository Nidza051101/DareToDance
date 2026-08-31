using NotificationService.Domain.NotificationRecord;

namespace NotificationService.Infrastructure.MessageQueue;
public static class RabbitMqTopology
{
    public const string Exchange = "notifications";
    public const string EmailQueue = "notifications.email";
    public const string DeadLetterExchange = "notifications.dlx";
    public const string EmailDeadLetterQueue = "notifications.email.dlq";
    public static string RoutingKeyFor(NotificationChannel channel) =>
        channel.ToString().ToLowerInvariant();
}
