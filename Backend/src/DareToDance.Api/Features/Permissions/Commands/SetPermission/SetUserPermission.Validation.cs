using FluentValidation;

namespace DareToDance.Api.Features.Permissions.Commands.SetUserPermission;

public sealed class SetUserPermissionCommandValidator : AbstractValidator<SetUserPermissionCommand>
{
    public SetUserPermissionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("PermissionId is required.");
    }
}