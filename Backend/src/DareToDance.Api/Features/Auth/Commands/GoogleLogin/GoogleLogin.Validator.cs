using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.GoogleLogin;

public sealed class GoogleLoginValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("Google ID token is required.");
    }
}