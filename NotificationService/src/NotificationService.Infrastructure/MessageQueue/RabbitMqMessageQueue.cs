using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Options;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.MessageQueue;

public sealed class RabbitMqMessageQueue : IMessageQueue, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;

    public RabbitMqMessageQueue(IOptions<RabbitMqSettings> options)
    {
        var settings = options.Value;
        _queueName = settings.QueueName;

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

        _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false).GetAwaiter().GetResult();
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
            exchange: string.Empty,   
            routingKey: _queueName,  
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