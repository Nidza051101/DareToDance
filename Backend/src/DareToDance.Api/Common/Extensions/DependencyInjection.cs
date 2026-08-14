using DareToDance.Api.Common.Behaviors;
using DareToDance.Api.Common.Endpoints;
using FluentValidation;

namespace DareToDance.Api.Common.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddEndpoints(typeof(DependencyInjection).Assembly);

        return services;
    }
}
