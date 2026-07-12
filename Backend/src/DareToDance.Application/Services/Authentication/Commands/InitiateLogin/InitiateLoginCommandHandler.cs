using DareToDance.Application.Common.Persistence;
using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Domain.Entities;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Commands.InitiateLogin;

public class InitiateLoginCommandHandler(IUserRepository userRepository, OtpIssuer otpIssuer)
    : IRequestHandler<InitiateLoginCommand, ErrorOr<OtpChallengeResult>>
{
    public async Task<ErrorOr<OtpChallengeResult>> Handle(InitiateLoginCommand command, CancellationToken cancellationToken)
    {
        // unknown email and cooldown suppression both return the same generic response,
        // so this endpoint reveals nothing about which accounts exist
        if (userRepository.GetUserByEmail(command.Email) is { } user)
        {
            await otpIssuer.IssueAsync(user, OtpPurpose.Login, cancellationToken);
        }

        return OtpChallengeResult.CodeSent;
    }
}
