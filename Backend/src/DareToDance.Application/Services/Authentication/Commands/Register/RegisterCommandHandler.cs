using DareToDance.Application.Common.Persistence;
using DareToDance.Application.Services.Authentication.Jwt;
using DareToDance.Domain.Common.Errors;
using DareToDance.Domain.Entities;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Commands.Register;

public class RegisterCommandHandler(IJwtTokenGenerator jwtTokenGenerator, IUserRepository userRepository)
    : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
{
    public Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (userRepository.GetUserByEmail(command.Email) is not null)
        {
            return Task.FromResult<ErrorOr<AuthenticationResult>>(Errors.User.DuplicateEmail);
        }

        var user = new User
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            Password = command.Password
        };

        userRepository.Add(user);

        var token = jwtTokenGenerator.GenerateToken(user.Id, command.FirstName, command.LastName);

        return Task.FromResult<ErrorOr<AuthenticationResult>>(
            new AuthenticationResult(user.Id, command.FirstName, command.LastName, command.Email, token));
    }
}
