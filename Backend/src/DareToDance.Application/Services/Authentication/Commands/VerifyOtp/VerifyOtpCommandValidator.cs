using FluentValidation;

namespace DareToDance.Application.Services.Authentication.Commands.VerifyOtp;

public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.Code).NotEmpty().Matches("^[0-9]{4,10}$");
    }
}
