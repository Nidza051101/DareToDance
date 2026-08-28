using DareToDance.Notifications.Grpc;
using DomainChannel = NotificationService.Domain.NotificationRecord.NotificationChannel;
using Grpc.Core;
using MediatR;

namespace NotificationService.Grpc.Features.Notifications.Commands.SendNotification;

public sealed class SendNotificationGrpcService(ISender sender)
    : global::DareToDance.Notifications.Grpc.NotificationService.NotificationServiceBase
{
    public override async Task<SendNotificationResponse> SendNotification(
        SendNotificationRequest request, ServerCallContext context)
    {
        var channel = request.Channel switch
        {
            Channel.Email => DomainChannel.Email,
            Channel.Sms => DomainChannel.Sms,
            Channel.Push => DomainChannel.Push,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Unknown channel.")),
        };

        var result = await sender.Send(
            new SendNotification.Command(
                request.Recipient,
                channel,
                request.Template,
                request.Variables.ToDictionary(kv => kv.Key, kv => kv.Value)),
            context.CancellationToken);

        return result.Match(
            success => new SendNotificationResponse
            {
                Accepted = true,
                TrackingId = success.TrackingId.ToString(),
            },
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, errors[0].Description)));
    }
}
