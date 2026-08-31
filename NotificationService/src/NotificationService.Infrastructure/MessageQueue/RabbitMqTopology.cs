using NotificationService.Domain.NotificationRecord;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.MessageQueue;
public static class RabbitMqTopology
{
    public const string Exchange = "notifications";
    public const string EmailQueue = "notifications.email";
    public const string DeadLetterExchange = "notifications.dlx";
    public const string EmailDeadLetterQueue = "notifications.email.dlq";
    public static string RoutingKeyFor(NotificationChannel channel) =>
        channel.ToString().ToLowerInvariant();

        public static async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(
            exchange: Exchange, type: ExchangeType.Direct, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: EmailQueue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: EmailQueue, exchange: Exchange,
            routingKey: RoutingKeyFor(NotificationChannel.Email),
            cancellationToken: cancellationToken);
    }
}
