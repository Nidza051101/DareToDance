using System.Diagnostics;
using DareToDance.Api.Common.Observability;
using MediatR;

namespace DareToDance.Api.Common.Behaviors;

public sealed class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly string SpanName = GetSpanName(typeof(TRequest).Name);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var activity = DareToDanceDiagnostics.ActivitySource.StartActivity(SpanName);

        activity?.SetTag("mediatr.request_type", typeof(TRequest).Name);

        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            if (activity is not null)
            {
                activity.AddException(exception);
                activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            }

            throw;
        }
    }

    private static string GetSpanName(string requestTypeName)
    {
        foreach (var suffix in new[] { "Command", "Query" })
        {
            if (requestTypeName.Length > suffix.Length &&
                requestTypeName.EndsWith(suffix, StringComparison.Ordinal))
            {
                return requestTypeName[..^suffix.Length];
            }
        }

        return requestTypeName;
    }
}
