using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByEmail;

public static partial class RequestLoginCodeByEmail
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(320);
        }
    }
}
