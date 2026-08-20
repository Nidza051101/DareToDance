using FluentValidation;

namespace DareToDance.Api.Features.Users.Commands.BlockUser;

public static partial class BlockUser
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id)
                .NotEmpty();
        }
    }
}
