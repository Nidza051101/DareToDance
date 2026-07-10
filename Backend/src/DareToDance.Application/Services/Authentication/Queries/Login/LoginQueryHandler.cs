using DareToDance.Application.Common.Persistence;
using DareToDance.Application.Services.Authentication.Jwt;
using DareToDance.Domain.Common.Errors;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Queries.Login;

public class LoginQueryHandler(IJwtTokenGenerator jwtTokenGenerator, IUserRepository userRepository)
    : IRequestHandler<LoginQuery, ErrorOr<AuthenticationResult>>
{
    public Task<ErrorOr<AuthenticationResult>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        if (userRepository.GetUserByEmail(query.Email) is not { } user || user.Password != query.Password)
        {
            return Task.FromResult<ErrorOr<AuthenticationResult>>(Errors.Authentication.InvalidCredentials);
        }

        var token = jwtTokenGenerator.GenerateToken(user.Id, user.FirstName, user.LastName);

        return Task.FromResult<ErrorOr<AuthenticationResult>>(
            new AuthenticationResult(user.Id, user.FirstName, user.LastName, user.Email, token));
    }
}
