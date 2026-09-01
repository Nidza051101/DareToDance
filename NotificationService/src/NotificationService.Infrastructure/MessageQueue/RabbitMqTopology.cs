using NotificationService.Domain.NotificationRecord;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.MessageQueue;
public static class RabbitMqTopology
{
    public const string Exchange = "notifications";
    public const string EmailQueue = "notifications.email";
    public const string DeadLetterExchange = "notifications.dlx";
    public const string EmailDeadLetterQueue = "notifications.email.dlq";

    // Poruke u DLQ-u se same brišu posle 7 dana — sprečava da red neograničeno raste.
    public const int DeadLetterTtlMs = 7 * 24 * 60 * 60 * 1000;

    public static string RoutingKeyFor(NotificationChannel channel) =>
        channel.ToString().ToLowerInvariant();

    public static async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken = default)
    {
        // --- mrtvo pismo: kuda odu poruke koje consumer nack-uje (requeue: false) ---
        await channel.ExchangeDeclareAsync(
            exchange: DeadLetterExchange, type: ExchangeType.Fanout, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: EmailDeadLetterQueue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-message-ttl"] = DeadLetterTtlMs },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: EmailDeadLetterQueue, exchange: DeadLetterExchange, routingKey: string.Empty,
            cancellationToken: cancellationToken);

        // --- glavni tok ---
        await channel.ExchangeDeclareAsync(
            exchange: Exchange, type: ExchangeType.Direct, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: EmailQueue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-dead-letter-exchange"] = DeadLetterExchange },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: EmailQueue, exchange: Exchange,
            routingKey: RoutingKeyFor(NotificationChannel.Email),
            cancellationToken: cancellationToken);
    }
}
