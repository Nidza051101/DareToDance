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
    private const ushort PrefetchCount = 100;

    private IChannel? _channel;
    private string? _consumerTag;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await connection.CreateChannelAsync(stoppingToken);

        try
        {
            await RabbitMqTopology.DeclareAsync(_channel, stoppingToken);
            await _channel.BasicQosAsync(
                prefetchSize: 0, prefetchCount: PrefetchCount, global: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageAsync;

            _consumerTag = await _channel.BasicConsumeAsync(
                queue: RabbitMqTopology.EmailQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // normalno gašenje (stoppingToken je otkazan)
        }
        finally
        {
            await ShutdownAsync();
        }
    }

    private async Task ShutdownAsync()
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            if (_consumerTag is not null)
            {
                await _channel.BasicCancelAsync(_consumerTag, cancellationToken: CancellationToken.None);
            }

            await _channel.CloseAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "NotificationConsumerShutdownError");
        }
        finally
        {
            await _channel.DisposeAsync();
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
            await DeadLetterAsync(ea, "deserialize-failed");
            return;
        }

        if (msg is null)
        {
            logger.LogError("NotificationDeserializedNull");
            await DeadLetterAsync(ea, "deserialize-null");
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
            await DeadLetterAsync(ea, "processing-failed");
        }
    }

    // nack(requeue: false) -> RabbitMQ preusmeri poruku na notifications.dlx -> DLQ.
    // Retry i dalje radi DB job (RetryFailedNotificationsJob), ovo je samo kanta
    // za pregled + poison poruke.
    private async Task DeadLetterAsync(BasicDeliverEventArgs ea, string reason)
    {
        logger.LogWarning(
            "NotificationDeadLettered reason={Reason} routingKey={RoutingKey}",
            reason, ea.RoutingKey);
        await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
    }
}
