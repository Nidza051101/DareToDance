using ErrorOr;
using MediatR;
using NotificationService.Grpc.Features.EmailChannel.Shared;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Services;

namespace NotificationService.Grpc.Features.EmailChannel.Commands.SendEmailViaGmail;

public static class SendEmailViaGmail
{
    public sealed record Command(
        Guid NotificationRecordId,
        string Recipient,
        string Template,
        IReadOnlyDictionary<string, string> Variables) : IRequest<ErrorOr<Success>>;

    
    public sealed class Handler(
        IEmailSender emailSender,
        NotificationDbContext dbContext,
        TimeProvider timeProvider,
        ILogger<Handler> logger) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var (subject, htmlBody) = EmailTemplates.Render(command.Template, command.Variables);

            var record = await dbContext.NotificationRecords.FindAsync(
                [command.NotificationRecordId], cancellationToken);

            try
            {
                await emailSender.SendAsync(
                    new EmailMessage(command.Recipient, subject, htmlBody), cancellationToken);

                record?.MarkSent(timeProvider.GetUtcNow().UtcDateTime);
                logger.LogInformation("EmailNotificationSent {NotificationRecordId}", command.NotificationRecordId);
            }
            catch (Exception ex)
            {
                record?.MarkFailed(ex.Message);
                logger.LogWarning(
                    ex, "EmailNotificationFailed {NotificationRecordId}", command.NotificationRecordId);
                throw;
            }
            finally
            {
                if (record is not null)
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            return Result.Success;
        }
    }
}
