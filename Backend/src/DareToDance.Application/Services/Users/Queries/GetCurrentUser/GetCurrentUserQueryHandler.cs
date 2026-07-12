using DareToDance.Application.Common.Security;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.Services.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(ICurrentUserProvider currentUserProvider)
    : IRequestHandler<GetCurrentUserQuery, ErrorOr<CurrentUser>>
{
    public Task<ErrorOr<CurrentUser>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        if (currentUserProvider.GetCurrentUser() is not { } user)
        {
            return Task.FromResult<ErrorOr<CurrentUser>>(
                Error.Unauthorized(description: "Authentication is required."));
        }

        return Task.FromResult<ErrorOr<CurrentUser>>(user);
    }
}
