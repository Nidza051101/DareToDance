using Microsoft.OpenApi.Models;

namespace DareToDance.Api.Common.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "DareToDance API", Version = "v1" });

            // Group by the first route segment ("users", "auth", ...) instead of
            // Swashbuckle's default (controller class name - would show up as
            // "GetUserByIdEndpoint", "RequestLoginCodeByEmailEndpoint", ...).
            options.TagActionsBy(api =>
            {
                var firstSegment = (api.RelativePath ?? string.Empty)
                    .Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(firstSegment))
                {
                    return [api.ActionDescriptor.RouteValues["controller"] ?? "Default"];
                }

                var tag = char.ToUpperInvariant(firstSegment[0]) + firstSegment[1..];

                return [tag];
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter the JWT obtained from /auth/login/verify (no 'Bearer ' prefix, it's added automatically)."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    []
                }
            });
        });

        return services;
    }

    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
}
