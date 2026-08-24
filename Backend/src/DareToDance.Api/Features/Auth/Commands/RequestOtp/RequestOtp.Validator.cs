using FluentValidation;

namespace DareToDance.Api.Features.Auth.Commands.RequestOtp;

public sealed class RequestOtpCommandValidator : AbstractValidator<RequestOtp.Command>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(320)
            .EmailAddress();
    }
}
