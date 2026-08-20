namespace DareToDance.Infrastructure.Services;

public interface IEmailSender
{
    Task SendLoginCodeAsync(string email, string code, CancellationToken cancellationToken);
}
