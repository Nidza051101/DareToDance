using DareToDance.Application.Common.Persistence;
using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Domain.Common.Errors;
using DareToDance.Domain.Entities;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Commands.Register;

public class RegisterCommandHandler(IUserRepository userRepository, OtpIssuer otpIssuer)
    : IRequestHandler<RegisterCommand, ErrorOr<OtpChallengeResult>>
{
    public async Task<ErrorOr<OtpChallengeResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (userRepository.GetUserByEmail(command.Email) is not null)
        {
            return Errors.User.DuplicateEmail;
        }

        var user = new User
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email
        };

        userRepository.Add(user);

        var issued = await otpIssuer.IssueAsync(user, OtpPurpose.Login, cancellationToken);
        if (issued.IsError)
        {
            return issued.Errors;
        }

        return OtpChallengeResult.CodeSent;
    }
}
