using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.CompleteGoogleRegistration;

public sealed class CompleteGoogleRegistrationCommandValidator : AbstractValidator<CompleteGoogleRegistration.Command>
{
    public CompleteGoogleRegistrationCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty();

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(30);
    }
}
