using Microsoft.Extensions.Logging;

namespace DareToDance.Infrastructure.Services;

// TODO: zameniti pravim SMS provajderom (npr. preko notifikacionog mikroservisa)
// kada bude dostupan. Za sad samo loguje kod, da bi se moglo testirati preko Swaggera.
internal sealed class ConsoleSmsSender(ILogger<ConsoleSmsSender> logger) : ISmsSender
{
    public Task SendLoginCodeAsync(string phone, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DEV] Login kod za telefon {Phone}: {Code}", phone, code);

        return Task.CompletedTask;
    }
}
