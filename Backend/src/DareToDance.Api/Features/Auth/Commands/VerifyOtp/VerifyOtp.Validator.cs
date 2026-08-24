using DareToDance.Infrastructure.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtp.Command>
{
    public VerifyOtpCommandValidator(IOptions<OtpSettings> otpOptions)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(320)
            .EmailAddress();

        // Malformed codes are rejected with 400 before the handler runs, so
        // they never touch the challenge row or burn an attempt.
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(otpOptions.Value.CodeLength)
            .Matches("^[0-9]+$");
    }
}
