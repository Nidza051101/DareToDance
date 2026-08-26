using System.Threading.Channels;

namespace NotificationService.Infrastructure.MessageQueue;

// PRIVREMENO — dovoljno da ceo tok (Enqueue -> Worker -> Email servis -> DB)
// stvarno radi lokalno, bez spoljnog broker-a, dok se ne izabere prava
// tehnologija (RabbitMQ, Azure Service Bus...). Poruke NE prežive restart
// procesa — to je poznato ograničenje ove privremene implementacije, ne
// nešto na šta treba da se osloni Failed Notification Scheduled Job (ta
// logika mora da čita iz baze, ne iz reda, baš zbog ovoga).
// Registruje se kao singleton (DependencyInjection.cs) da isti Channel
// dele i EnqueueAsync (piše) i Reader (čita, u NotificationQueueConsumer).
public sealed class InMemoryMessageQueue : IMessageQueue
{
    private readonly Channel<QueuedNotification> _channel =
        Channel.CreateUnbounded<QueuedNotification>();

    public ChannelReader<QueuedNotification> Reader => _channel.Reader;

    public async Task EnqueueAsync(QueuedNotification notification, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(notification, cancellationToken);
    }
}
