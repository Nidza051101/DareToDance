using DareToDance.Application.Common.Persistence;
using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Domain.Entities;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Commands.ResendOtp;

public class ResendOtpCommandHandler(IUserRepository userRepository, OtpIssuer otpIssuer)
    : IRequestHandler<ResendOtpCommand, ErrorOr<OtpChallengeResult>>
{
    public async Task<ErrorOr<OtpChallengeResult>> Handle(ResendOtpCommand command, CancellationToken cancellationToken)
    {
        if (userRepository.GetUserByEmail(command.Email) is not { } user)
        {
            return OtpChallengeResult.CodeSent;
        }

        var issued = await otpIssuer.IssueAsync(user, OtpPurpose.Login, cancellationToken);
        if (issued.IsError)
        {
            // surfacing the cooldown helps a legitimate user; it only fires for existing accounts
            return issued.Errors;
        }

        return OtpChallengeResult.CodeSent;
    }
}
