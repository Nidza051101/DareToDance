using Microsoft.Extensions.Logging;

namespace DareToDance.Infrastructure.Services;

// TODO: replace with a real SMS provider (e.g. via the notification microservice)
// once it's available. For now it just logs the code, so it can be tested via Swagger.
internal sealed class ConsoleSmsSender(ILogger<ConsoleSmsSender> logger) : ISmsSender
{
    public Task SendLoginCodeAsync(string phone, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DEV] Login code for phone {Phone}: {Code}", phone, code);

        return Task.CompletedTask;
    }
}
