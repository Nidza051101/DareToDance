using System.Text.Json;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Infrastructure.MessageQueue;
using NotificationService.Infrastructure.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace NotificationService.Grpc.IntegrationTests.Features.Notifications;

public class RabbitMqPublisherTests : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder()
        .WithImage("rabbitmq:3")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task EnqueueAsync_MessageArrivesOnQueue()
    {
        var settings = Options.Create(new RabbitMqSettings
        {
            Host = _container.Hostname,
            Port = _container.GetMappedPublicPort(5672),
            Username = "guest",
            Password = "guest",
            VirtualHost = "/",
            QueueName = "notifications"
        });

        var queue = new RabbitMqMessageQueue(settings);

        var notification = new QueuedNotification(
            NotificationRecordId: Guid.NewGuid(),
            Recipient: "test@example.com",
            Channel: NotificationChannel.Email,
            Template: "OtpCode",
            Variables: new Dictionary<string, string> { ["code"] = "123456" }
        );

        await queue.EnqueueAsync(notification, CancellationToken.None);

        // Assert
        var factory = new ConnectionFactory
        {
            HostName = _container.Hostname,
            Port = _container.GetMappedPublicPort(5672),
            UserName = "guest",
            Password = "guest"
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        var result = await channel.BasicGetAsync("notifications", autoAck: true);

        Assert.NotNull(result);
        var received = JsonSerializer.Deserialize<QueuedNotification>(result.Body.Span);
        Assert.Equal(notification.NotificationRecordId, received!.NotificationRecordId);
    }
}