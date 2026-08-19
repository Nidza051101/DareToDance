using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.VerifyLoginCode;

public sealed class VerifyLoginCodeCommandValidator : AbstractValidator<VerifyLoginCodeCommand>
{
    public VerifyLoginCodeCommandValidator()
    {
        RuleFor(c => c.Recipient)
            .NotEmpty()
            .MaximumLength(320);

        RuleFor(c => c.Code)
            .NotEmpty()
            .Length(6);
    }
}
