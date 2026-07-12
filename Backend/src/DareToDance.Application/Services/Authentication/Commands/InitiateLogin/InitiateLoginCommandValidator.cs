using FluentValidation;

namespace DareToDance.Application.Services.Authentication.Commands.InitiateLogin;

public class InitiateLoginCommandValidator : AbstractValidator<InitiateLoginCommand>
{
    public InitiateLoginCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}
