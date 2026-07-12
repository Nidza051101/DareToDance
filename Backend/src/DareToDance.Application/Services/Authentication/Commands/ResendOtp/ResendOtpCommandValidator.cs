using FluentValidation;

namespace DareToDance.Application.Services.Authentication.Commands.ResendOtp;

public class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}
