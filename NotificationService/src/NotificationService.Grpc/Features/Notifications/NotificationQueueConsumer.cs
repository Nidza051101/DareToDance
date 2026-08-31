using MediatR;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Grpc.Features.EmailChannel.Commands.SendEmailViaGmail;
using NotificationService.Infrastructure.MessageQueue;

namespace NotificationService.Grpc.Features.Notifications;

public sealed class NotificationQueueConsumer(
    InMemoryMessageQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationQueueConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in queue.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            try
            {
                switch (item.Channel)
                {
                    case NotificationChannel.Email:
                        await sender.Send(
                            new SendEmailViaGmail.Command(
                                item.NotificationRecordId, item.Recipient, item.Template, item.Variables),
                            stoppingToken);
                        break;

                    case NotificationChannel.Sms:
                    case NotificationChannel.Push:
                        logger.LogWarning(
                            "ChannelNotImplemented {Channel} for {NotificationRecordId}",
                            item.Channel, item.NotificationRecordId);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex, "NotificationProcessingFailed {NotificationRecordId}", item.NotificationRecordId);
            }
        }
    }
}
