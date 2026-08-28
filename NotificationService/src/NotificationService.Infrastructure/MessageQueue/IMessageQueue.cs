using System.Text.Json.Serialization;
using NotificationService.Domain.NotificationRecord;

namespace NotificationService.Infrastructure.MessageQueue;

public interface IMessageQueue {
    Task EnqueueAsync(QueuedNotification notification, CancellationToken cancellationToken);
}
public sealed record QueuedNotification(
    Guid NotificationRecordId,
    string Recipient,
    [property: JsonConverter(typeof(JsonStringEnumConverter<NotificationChannel>))]
    NotificationChannel Channel,
    string Template,
    Dictionary<string, string> Variables,
    int SchemaVersion = 1);
