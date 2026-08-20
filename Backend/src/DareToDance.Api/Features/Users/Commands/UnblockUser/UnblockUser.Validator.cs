using FluentValidation;

namespace DareToDance.Api.Features.Users.Commands.UnblockUser;

public static partial class UnblockUser
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
