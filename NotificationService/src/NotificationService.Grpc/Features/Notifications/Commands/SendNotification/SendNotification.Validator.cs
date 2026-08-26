using FluentValidation;

namespace NotificationService.Grpc.Features.Notifications.Commands.SendNotification;

public sealed class SendNotificationCommandValidator : AbstractValidator<SendNotification.Command>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.Recipient).NotEmpty().MaximumLength(320);
        RuleFor(x => x.Template).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Channel).IsInEnum();
    }
}
