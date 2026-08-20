using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.VerifyLoginCode;

public static partial class VerifyLoginCode
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Recipient)
                .NotEmpty()
                .MaximumLength(320);

            RuleFor(c => c.Code)
                .NotEmpty()
                .Length(6);
        }
    }
}
