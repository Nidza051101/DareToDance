using ErrorOr;
using MediatR;
using NotificationService.Domain.NotificationRecord;
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
            var record = await dbContext.NotificationRecords.FindAsync(
                [command.NotificationRecordId], cancellationToken);

            // Zapis ne postoji — nema šta da se pošalje ni upiše; ack (return), ne
            // vrti poruku unedogled.
            if (record is null)
            {
                logger.LogWarning("EmailNotificationRecordMissing {NotificationRecordId}", command.NotificationRecordId);
                return Result.Success;
            }

            // Već poslato — RabbitMQ ume da isporuči poruku više puta (redelivery,
            // DB retry). Ne šalji drugi put; ack.
            if (record.Status == NotificationStatus.Sent)
            {
                logger.LogInformation("EmailNotificationAlreadySent {NotificationRecordId}", command.NotificationRecordId);
                return Result.Success;
            }

            var (subject, htmlBody) = EmailTemplates.Render(command.Template, command.Variables);

            try
            {
                await emailSender.SendAsync(
                    new EmailMessage(command.Recipient, subject, htmlBody), cancellationToken);

                record.MarkSent(timeProvider.GetUtcNow().UtcDateTime);
                logger.LogInformation("EmailNotificationSent {NotificationRecordId}", command.NotificationRecordId);
            }
            catch (Exception ex)
            {
                record.MarkFailed(ex.Message);
                logger.LogWarning(
                    ex, "EmailNotificationFailed {NotificationRecordId}", command.NotificationRecordId);
                throw;
            }
            finally
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return Result.Success;
        }
    }
}
