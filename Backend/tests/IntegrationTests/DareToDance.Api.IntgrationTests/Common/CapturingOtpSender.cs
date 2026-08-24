using System.Threading.Channels;
using DareToDance.Infrastructure.Services;

namespace DareToDance.Api.IntgrationTests.Common;

// The only sanctioned way tests learn a plaintext code. Reading is awaitable
// so the tests stay correct when sending moves onto a background queue in the
// real-notification iteration.
public sealed class CapturingOtpSender : IOtpSender
{
    private readonly Channel<OtpNotification> _notifications = Channel.CreateUnbounded<OtpNotification>();

    public Task SendAsync(OtpNotification notification, CancellationToken cancellationToken)
    {
        _notifications.Writer.TryWrite(notification);
        return Task.CompletedTask;
    }

    public async Task<OtpNotification> WaitForNotificationAsync(TimeSpan? timeout = null)
    {
        using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(5));
        return await _notifications.Reader.ReadAsync(cts.Token);
    }

    public bool TryTakeNotification(out OtpNotification? notification)
    {
        var taken = _notifications.Reader.TryRead(out var read);
        notification = read;
        return taken;
    }
}
