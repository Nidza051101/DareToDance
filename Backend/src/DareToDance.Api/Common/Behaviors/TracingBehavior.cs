using System.Diagnostics;
using DareToDance.Api.Common.Observability;
using MediatR;

namespace DareToDance.Api.Common.Behaviors;

public sealed class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly string SpanName = GetSpanName(typeof(TRequest));

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

    private static string GetSpanName(Type requestType)
    {
        // Command/Query tipovi zive ugnjezdeni unutar feature klase (npr. CreateUser.Command),
        // pa samo ime tipa ("Command") nije dovoljno opisno - uzmi ime obuhvatajuce klase.
        if (requestType.IsNested && requestType.DeclaringType is not null)
        {
            return requestType.DeclaringType.Name;
        }

        var requestTypeName = requestType.Name;

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
