using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Infrastructure.Options;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.MessageQueue;

public sealed class RabbitMqMessageQueue : IMessageQueue, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqMessageQueue(IOptions<RabbitMqSettings> options)
    {
        var settings = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            VirtualHost = settings.VirtualHost,
            UserName = settings.Username,
            Password = settings.Password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync(new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true
        )).GetAwaiter().GetResult();

        // Isti exchange / red / DLX koje deklariše i consumer. Deklaracija je
        // idempotentna, pa je bezbedno da je zovu obe strane — publisher je
        // ovde da bi exchange postojao i kad consumer još nije startovao.
        RabbitMqTopology.DeclareAsync(_channel).GetAwaiter().GetResult();
    }

    public async Task EnqueueAsync(QueuedNotification notification, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(notification);

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await _channel.BasicPublishAsync(
            exchange: RabbitMqTopology.Exchange,
            routingKey: RabbitMqTopology.RoutingKeyFor(notification.Channel),
            mandatory: true,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
