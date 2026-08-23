using ErrorOr;
using MediatR;

namespace DareToDance.Api.Common.Behaviors;

// The domain returns Error.Unexpected instead of throwing for invariant
// violations, so those bugs no longer pass through the exception-logging
// path. This behavior restores their loudness: any response carrying an
// Unexpected error is logged as an error before it leaves the pipeline.
public sealed class UnexpectedErrorLoggingBehavior<TRequest, TResponse>(
    ILogger<UnexpectedErrorLoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    // Command/Query types are nested inside their feature class, so the bare
    // type name ("Command") says nothing — use the feature name instead.
    private static readonly string RequestName =
        typeof(TRequest).DeclaringType?.Name ?? typeof(TRequest).Name;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (response.IsError && response.Errors is { } errors)
        {
            var unexpected = errors.Where(e => e.Type == ErrorType.Unexpected).ToList();

            if (unexpected.Count > 0)
            {
                logger.LogError(
                    "UnexpectedDomainError in {RequestName}: {ErrorCodes}",
                    RequestName,
                    string.Join(", ", unexpected.Select(e => e.Code)));
            }
        }

        return response;
    }
}
