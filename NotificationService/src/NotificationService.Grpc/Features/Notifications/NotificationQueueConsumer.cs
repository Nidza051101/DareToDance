using MediatR;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Grpc.Features.EmailChannel.Commands.SendEmailViaGmail;
using NotificationService.Infrastructure.MessageQueue;

namespace NotificationService.Grpc.Features.Notifications;

// Ovo je "Radnik"/"Worker" sa sekvencijalnog dijagrama u artifact-u — čita iz
// IMessageQueue.Reader i prosleđuje na pravi kanal. Za sada zna samo za
// Email (prioritet ove faze); SmsChannel/PushChannel su prazni skeleti dok
// ne dođu na red. Registrovan kao IHostedService u Program.cs.
public sealed class NotificationQueueConsumer(
    InMemoryMessageQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationQueueConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in queue.Reader.ReadAllAsync(stoppingToken))
        {
            // Novi scope po poruci — Handler-i ispod (SendEmailViaGmail) mogu
            // zavisiti od scoped servisa (npr. DbContext) kao i svaki drugi
            // MediatR handler u ovom projektu.
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
                        // TODO: SmsChannel / PushChannel — v. Features/SmsChannel,
                        // Features/PushChannel (namerno prazni za sada).
                        logger.LogWarning(
                            "ChannelNotImplemented {Channel} for {NotificationRecordId}",
                            item.Channel, item.NotificationRecordId);
                        break;
                }
            }
            catch (Exception ex)
            {
                // Ne obaraj ceo Worker zbog jedne poruke — Failed Notification
                // Scheduled Job (kasnije) je taj koji odlučuje o ponovnom pokušaju.
                logger.LogError(
                    ex, "NotificationProcessingFailed {NotificationRecordId}", item.NotificationRecordId);
            }
        }
    }
}
