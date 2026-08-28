using System.Threading.Channels;

namespace NotificationService.Infrastructure.MessageQueue;

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
