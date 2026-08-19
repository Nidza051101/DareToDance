using Microsoft.Extensions.Logging;

namespace DareToDance.Infrastructure.Services;

// TODO: zameniti pravim email provajderom (npr. preko notifikacionog mikroservisa)
// kada bude dostupan. Za sad samo loguje kod, da bi se moglo testirati preko Swaggera.
internal sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendLoginCodeAsync(string email, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DEV] Login kod za email {Email}: {Code}", email, code);

        return Task.CompletedTask;
    }
}
