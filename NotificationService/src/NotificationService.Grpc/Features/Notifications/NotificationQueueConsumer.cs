using System.Text.Json;
using MediatR;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Grpc.Features.EmailChannel.Commands.SendEmailViaGmail;
using NotificationService.Infrastructure.MessageQueue;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Grpc.Features.Notifications;

public sealed class NotificationQueueConsumer(
    RabbitMqConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationQueueConsumer> logger) : BackgroundService
{
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await connection.CreateChannelAsync(stoppingToken);
        await RabbitMqTopology.DeclareAsync(_channel, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: RabbitMqTopology.EmailQueue,
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // gašenje — uredno zatvaranje kanala dolazi u 1.5
        }
    }

    private async Task OnMessageAsync(object _, BasicDeliverEventArgs ea)
    {
        QueuedNotification? msg;
        try
        {
            msg = JsonSerializer.Deserialize<QueuedNotification>(ea.Body.Span);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "NotificationDeserializeFailed");
            return;
        }

        if (msg is null)
        {
            logger.LogError("NotificationDeserializedNull");
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            switch (msg.Channel)
            {
                case NotificationChannel.Email:
                    await mediator.Send(new SendEmailViaGmail.Command(
                        msg.NotificationRecordId, msg.Recipient, msg.Template, msg.Variables));
                    break;

                case NotificationChannel.Sms:
                case NotificationChannel.Push:
                    logger.LogWarning(
                        "ChannelNotImplemented {Channel} for {NotificationRecordId}",
                        msg.Channel, msg.NotificationRecordId);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NotificationProcessingFailed {NotificationRecordId}", msg.NotificationRecordId);
        }
    }
}
