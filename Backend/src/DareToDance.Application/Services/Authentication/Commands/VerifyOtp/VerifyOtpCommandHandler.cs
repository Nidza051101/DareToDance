using DareToDance.Application.Common.Persistence;
using DareToDance.Application.Common.Services;
using DareToDance.Application.Services.Authentication.Jwt;
using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Domain.Common.Errors;
using DareToDance.Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Options;

namespace DareToDance.Application.Services.Authentication.Commands.VerifyOtp;

public class VerifyOtpCommandHandler(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IOtpCodeGenerator otpCodeGenerator,
    IJwtTokenGenerator jwtTokenGenerator,
    IDateTimeProvider dateTimeProvider,
    IOptions<OtpSettings> otpOptions)
    : IRequestHandler<VerifyOtpCommand, ErrorOr<AuthenticationResult>>
{
    private readonly OtpSettings _settings = otpOptions.Value;

    public Task<ErrorOr<AuthenticationResult>> Handle(VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Verify(command));
    }

    private ErrorOr<AuthenticationResult> Verify(VerifyOtpCommand command)
    {
        if (userRepository.GetUserByEmail(command.Email) is not { } user)
        {
            return Errors.Otp.InvalidCode;
        }

        var otp = otpRepository.GetLatestByUserId(user.Id, OtpPurpose.Login);
        if (otp is null || otp.IsConsumed)
        {
            return Errors.Otp.InvalidCode;
        }

        if (dateTimeProvider.UtcNow >= otp.ExpiresAt)
        {
            return Errors.Otp.Expired;
        }

        if (otp.FailedAttempts >= _settings.MaxFailedAttempts)
        {
            return Errors.Otp.TooManyAttempts;
        }

        if (!otpCodeGenerator.Matches(command.Code, otp.CodeHash))
        {
            otp.FailedAttempts++;
            otpRepository.Update(otp);
            return Errors.Otp.InvalidCode;
        }

        otp.IsConsumed = true;
        otpRepository.Update(otp);

        var token = jwtTokenGenerator.GenerateToken(
            new TokenSubject(user.Id, user.FirstName, user.LastName, user.Email, Roles: []));

        return new AuthenticationResult(user.Id, user.FirstName, user.LastName, user.Email, token);
    }
}
