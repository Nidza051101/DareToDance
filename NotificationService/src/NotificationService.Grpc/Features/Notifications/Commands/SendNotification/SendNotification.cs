using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationRecordEntity = NotificationService.Domain.NotificationRecord.NotificationRecord;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Infrastructure.MessageQueue;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Grpc.Features.Notifications.Commands.SendNotification;

public static class SendNotification
{
    public sealed record Command(
        string Recipient,
        NotificationChannel Channel,
        string Template,
        IReadOnlyDictionary<string, string> Variables) : IRequest<ErrorOr<Result>>;

    public sealed record Result(Guid TrackingId);

    public sealed class Handler(
        NotificationDbContext dbContext,
        IMessageQueue messageQueue,
        TimeProvider timeProvider,
        ILogger<Handler> logger)
        : IRequestHandler<Command, ErrorOr<Result>>
    {
        public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken cancellationToken)
        {
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;

            var recordResult = NotificationRecordEntity.Create(
                command.Recipient, command.Channel, command.Template, command.Variables, utcNow);

            if (recordResult.IsError)
            {
                return recordResult.Errors;
            }

            var record = recordResult.Value;

            dbContext.NotificationRecords.Add(record);
            await dbContext.SaveChangesAsync(cancellationToken);

            await messageQueue.EnqueueAsync(
                new QueuedNotification(
                    record.Id, command.Recipient, command.Channel, command.Template, command.Variables),
                cancellationToken);

            logger.LogInformation(
                "NotificationQueued {NotificationRecordId} for {Recipient} via {Channel}",
                record.Id, command.Recipient, command.Channel);

            return new Result(record.Id);
        }
    }
}
