using NotificationService.Domain.NotificationRecord;

namespace NotificationService.Infrastructure.MessageQueue;

// Apstrakcija namerno tanka i tehnologija-agnostička — RabbitMQ/Azure Service
// Bus/itd. nije odlučeno (v. artifact, "za mentora" napomena). Handler-i i
// Worker zavise samo od ovog interfejsa, nikad direktno od broker klijenta.
public interface IMessageQueue
{
    Task EnqueueAsync(QueuedNotification notification, CancellationToken cancellationToken);
}

// Nosi Recipient/Template/Variables direktno (ne samo Id) da obrada ne zavisi
// od toga da baza već radi — privremeno rešenje dok DB provajder nije
// izabran, v. napomenu u NotificationDbContext.cs. Kad provajder stigne, ovo
// se može svesti na samo NotificationRecordId + čitanje iz baze u Worker-u.
public sealed record QueuedNotification(
    Guid NotificationRecordId,
    string Recipient,
    NotificationChannel Channel,
    string Template,
    IReadOnlyDictionary<string, string> Variables);
