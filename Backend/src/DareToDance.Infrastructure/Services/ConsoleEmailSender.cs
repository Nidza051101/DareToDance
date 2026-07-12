using DareToDance.Application.Common.Services;
using Microsoft.Extensions.Logging;

namespace DareToDance.Infrastructure.Services;

/// <summary>Development stand-in for a real email provider: writes the code to the console.</summary>
public class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendOtpAsync(string email, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("OTP for {Email}: {Code}", email, code);

        return Task.CompletedTask;
    }
}
