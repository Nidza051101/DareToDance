using ErrorOr;
using FluentValidation;
using MediatR;

namespace NotificationService.Grpc.Common.Behaviors;

// Isti obrazac kao DareToDance.Api.Common.Behaviors.ValidationBehavior u D2D
// Backend-u — kopirano namerno, ne deljeno preko project reference-a, jer su
// ovo dva nezavisna deployment-a (v. artifact "Notification gRPC Flow", deo 6).
public sealed class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validator is null)
        {
            return await next();
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next();
        }

        var errors = validationResult.Errors
            .ConvertAll(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage));

        return (dynamic)errors;
    }
}
