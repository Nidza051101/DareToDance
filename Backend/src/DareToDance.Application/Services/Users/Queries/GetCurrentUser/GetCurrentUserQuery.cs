using DareToDance.Application.Common.Security;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Users.Queries.GetCurrentUser;

[Authorize]
public record GetCurrentUserQuery : IRequest<ErrorOr<CurrentUser>>;
