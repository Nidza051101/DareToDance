using FluentValidation;

namespace DareToDance.Api.Features.Users.Commands.UnblockUser;

public sealed class UnblockUserCommandValidator : AbstractValidator<UnblockUserCommand>
{
    public UnblockUserCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty();
    }
}
