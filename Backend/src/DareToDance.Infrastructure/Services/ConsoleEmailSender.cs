using Microsoft.Extensions.Logging;

namespace DareToDance.Infrastructure.Services;

// TODO: replace with a real email provider (e.g. via the notification microservice)
// once it's available. For now it just logs the code, so it can be tested via Swagger.
internal sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendLoginCodeAsync(string email, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DEV] Login code for email {Email}: {Code}", email, code);

        return Task.CompletedTask;
    }
}
