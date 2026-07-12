using System.Reflection;
using DareToDance.Application.Common.Behaviors;
using DareToDance.Application.Services.Authentication.Otp;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DareToDance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Add MediatR pipeline behaviors (registration order = execution order:
        // reject unauthorized callers before spending effort validating their input)
        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(AuthorizationBehavior<,>)
        );
        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>)
        );

        services.AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly()]);

        services.AddScoped<OtpIssuer>();
        
        
        return services;
    }
}
