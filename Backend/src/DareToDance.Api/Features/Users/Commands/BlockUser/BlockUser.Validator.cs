using FluentValidation;

namespace DareToDance.Api.Features.Users.Commands.BlockUser;

public sealed class BlockUserCommandValidator : AbstractValidator<BlockUserCommand>
{
    public BlockUserCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty();
    }
}
