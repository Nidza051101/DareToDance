using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByEmail;

public sealed class RequestLoginCodeByEmailCommandValidator : AbstractValidator<RequestLoginCodeByEmailCommand>
{
    public RequestLoginCodeByEmailCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);
    }
}
