namespace DareToDance.Application.Common.Services;

public interface IEmailSender
{
    Task SendOtpAsync(string email, string code, CancellationToken cancellationToken);
}
