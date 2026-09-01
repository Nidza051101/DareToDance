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
    private readonly RabbitMqContainer _container =
        new RabbitMqBuilder("rabbitmq:3-management").Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task EnqueueAsync_MessageArrivesOnEmailQueue()
    {
        var uri = new Uri(_container.GetConnectionString());
        var credentials = uri.UserInfo.Split(':');

        var settings = Options.Create(new RabbitMqSettings
        {
            Host = uri.Host,
            Port = uri.Port,
            Username = credentials[0],
            Password = credentials[1],
            VirtualHost = "/"
        });

        await using var queue = new RabbitMqMessageQueue(settings);

        var notification = new QueuedNotification(
            NotificationRecordId: Guid.NewGuid(),
            Recipient: "test@example.com",
            Channel: NotificationChannel.Email,
            Template: "OtpCode",
            Variables: new Dictionary<string, string> { ["code"] = "123456" });

        await queue.EnqueueAsync(notification, CancellationToken.None);

        var factory = new ConnectionFactory { Uri = uri };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        BasicGetResult? result = null;
        for (var attempt = 0; attempt < 20 && result is null; attempt++)
        {
            result = await channel.BasicGetAsync(RabbitMqTopology.EmailQueue, autoAck: true);
            if (result is null)
            {
                await Task.Delay(100);
            }
        }

        Assert.NotNull(result);
        var received = JsonSerializer.Deserialize<QueuedNotification>(result!.Body.Span);
        Assert.Equal(notification.NotificationRecordId, received!.NotificationRecordId);
    }
}
