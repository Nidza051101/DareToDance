using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.RefreshAccessToken;

public static partial class RefreshAccessToken
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.RefreshToken)
                .NotEmpty();
        }
    }
}
