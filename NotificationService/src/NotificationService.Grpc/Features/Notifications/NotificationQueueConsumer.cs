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
    private const ushort PrefetchCount = 10;

    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await connection.CreateChannelAsync(stoppingToken);
        await RabbitMqTopology.DeclareAsync(_channel, stoppingToken);
        await _channel.BasicQosAsync(
            prefetchSize: 0, prefetchCount: PrefetchCount, global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: RabbitMqTopology.EmailQueue,
            autoAck: false,
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
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        if (msg is null)
        {
            logger.LogError("NotificationDeserializedNull");
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
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

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NotificationProcessingFailed {NotificationRecordId}", msg.NotificationRecordId);
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }
}
