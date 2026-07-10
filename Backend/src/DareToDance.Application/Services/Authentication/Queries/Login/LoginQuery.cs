using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Authentication.Queries.Login;

public record LoginQuery(
    string Email,
    string Password
) : IRequest<ErrorOr<AuthenticationResult>>;
