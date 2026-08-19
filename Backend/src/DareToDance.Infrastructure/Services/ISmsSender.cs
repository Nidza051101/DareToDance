namespace DareToDance.Infrastructure.Services;

public interface ISmsSender
{
    Task SendLoginCodeAsync(string phone, string code, CancellationToken cancellationToken);
}
