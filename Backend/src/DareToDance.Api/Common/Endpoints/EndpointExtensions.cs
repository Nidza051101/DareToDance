using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DareToDance.Api.Common.Endpoints;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var descriptors = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                           && type.IsAssignableTo(typeof(IApiEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IApiEndpoint), type));

        services.TryAddEnumerable(descriptors);

        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        // scope umesto root providera — endpoint sme da injektuje scoped servise
        using var scope = app.Services.CreateScope();
        var endpoints = scope.ServiceProvider.GetRequiredService<IEnumerable<IApiEndpoint>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.Map(app);
        }

        return app;
    }
}
