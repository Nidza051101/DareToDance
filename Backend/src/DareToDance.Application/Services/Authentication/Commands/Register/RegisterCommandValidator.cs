using FluentValidation;

namespace DareToDance.Application.Services.Authentication.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}
