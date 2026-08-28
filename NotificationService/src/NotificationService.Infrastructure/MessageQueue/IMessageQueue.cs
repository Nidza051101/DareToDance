using NotificationService.Domain.NotificationRecord;

namespace NotificationService.Infrastructure.MessageQueue;

public interface IMessageQueue {
    Task EnqueueAsync(QueuedNotification notification, CancellationToken cancellationToken);
}

public sealed record QueuedNotification(
    Guid NotificationRecordId,
    string Recipient,
    NotificationChannel Channel,
    string Template,
    IReadOnlyDictionary<string, string> Variables);
