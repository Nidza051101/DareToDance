using System.Reflection;
using DareToDance.Application.Common.Security;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse>(ICurrentUserProvider currentUserProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    // resolved once per closed generic type, so reflection never runs on the hot path
    private static readonly AuthorizeAttribute? Authorization =
        typeof(TRequest).GetCustomAttribute<AuthorizeAttribute>();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (Authorization is null)
        {
            return await next(cancellationToken);
        }

        if (currentUserProvider.GetCurrentUser() is not { } user)
        {
            return (dynamic)Error.Unauthorized(description: "Authentication is required.");
        }

        var requiredRoles = Authorization.Roles?.Split(
            ',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];

        if (requiredRoles.Any(role => !user.Roles.Contains(role)))
        {
            return (dynamic)Error.Forbidden(description: "Insufficient permissions.");
        }

        return await next(cancellationToken);
    }
}
