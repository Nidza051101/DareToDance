using Microsoft.Extensions.Logging;

namespace DareToDance.Infrastructure.Services;

// DEV ONLY: writes the plaintext code to the console/log — a deliberate,
// environment-gated exception to the no-secrets-in-logs rule. Registration
// throws outside Development, so this can never ship as the real sender.
internal sealed class ConsoleOtpSender(ILogger<ConsoleOtpSender> logger) : IOtpSender
{
    public Task SendAsync(OtpNotification notification, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "DEV ONLY — OTP code for {Email}: {Code} (expires {ExpiresAtUtc:O})",
            notification.Email,
            notification.Code,
            notification.ExpiresAtUtc);

        return Task.CompletedTask;
    }
}
