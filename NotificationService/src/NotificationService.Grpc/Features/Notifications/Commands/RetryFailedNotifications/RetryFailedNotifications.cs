using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Infrastructure.MessageQueue;
using NotificationService.Infrastructure.Options;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Grpc.Features.Notifications.Commands.RetryFailedNotifications;

public static class RetryFailedNotifications
{
    public sealed record Command : IRequest<ErrorOr<Success>>;

    public sealed class Handler(
        NotificationDbContext dbContext,
        IMessageQueue messageQueue,
        IOptions<RetryPolicySettings> retryOptions,
        ILogger<Handler> logger) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var maxAttempts = retryOptions.Value.MaxAttempts;

            var candidates = await dbContext.NotificationRecords
                .Where(r => r.Status == NotificationStatus.Failed && r.RetryCount < maxAttempts)
                .ToListAsync(cancellationToken);

            foreach (var record in candidates)
            {
                record.ResetForRetry();

                await messageQueue.EnqueueAsync(
                    new QueuedNotification(
                        record.Id, record.Recipient, record.Channel, record.Template, record.Variables),
                    cancellationToken);

                logger.LogInformation(
                    "NotificationRequeued {NotificationRecordId} (failed {RetryCount} of {MaxAttempts} times)",
                    record.Id, record.RetryCount, maxAttempts);
            }

            if (candidates.Count > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return Result.Success;
        }
    }
}

public sealed class RetryFailedNotificationsJob : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
