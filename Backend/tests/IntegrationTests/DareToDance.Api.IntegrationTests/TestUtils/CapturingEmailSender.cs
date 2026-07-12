using DareToDance.Application.Common.Services;

namespace DareToDance.Api.IntegrationTests.TestUtils;

public class CapturingEmailSender : IEmailSender
{
    public record SentEmail(string Email, string Code);

    public List<SentEmail> Sent { get; } = [];

    public Task SendOtpAsync(string email, string code, CancellationToken cancellationToken)
    {
        Sent.Add(new SentEmail(email, code));
        return Task.CompletedTask;
    }
}
