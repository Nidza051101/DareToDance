using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NotificationService.Infrastructure.Options;

namespace NotificationService.Infrastructure.Services;

public sealed class GmailEmailSender(
    IOptions<EmailSettings> emailOptions,
    ILogger<GmailEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = emailOptions.Value;

        var mime = new MimeMessage();
        mime.From.Add(MailboxAddress.Parse(settings.GmailAddress));
        mime.To.Add(MailboxAddress.Parse(message.Recipient));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                settings.SmtpHost, settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);

            await client.AuthenticateAsync(settings.GmailAddress, settings.AppPassword, cancellationToken);

            await client.SendAsync(mime, cancellationToken);

            logger.LogInformation("EmailSent to {Recipient}", message.Recipient);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "EmailSendFailed to {Recipient}", message.Recipient);
            throw;
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
