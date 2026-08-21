using FluentValidation;

namespace DareToDance.Api.Features.Permissions.Commands.AssignPermission;

public sealed class AssignPermissionCommandValidator
    : AbstractValidator<AssignPermissionCommand>
{
    public AssignPermissionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("PermissionId is required.");
    }
}