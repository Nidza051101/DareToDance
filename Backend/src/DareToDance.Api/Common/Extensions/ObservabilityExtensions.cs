using System.Reflection;
using DareToDance.Api.Common.Observability;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace DareToDance.Api.Common.Extensions;

public static class ObservabilityExtensions
{
    private const string ServiceName = "daretodance-api";

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Host.UseSerilog(
            (context, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext();

                if (context.HostingEnvironment.IsDevelopment())
                {
                    loggerConfiguration.WriteTo.Console();
                }
                else
                {
                    // compact JSON — spremno za App Service log stream
                    loggerConfiguration.WriteTo.Console(new CompactJsonFormatter());
                }
            },
            writeToProviders: true);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(ServiceName, serviceVersion: GetServiceVersion())
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                }))
            .WithTracing(tracing => tracing
                .AddSource(DareToDanceDiagnostics.ActivitySourceName)
                .AddAspNetCoreInstrumentation(options =>
                    options.Filter = httpContext => !IsHealthCheckRequest(httpContext.Request.Path))
                .AddHttpClientInstrumentation()
                .AddNpgsql())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithLogging(_ => { }, options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
            })
            .UseOtlpExporter();

        var connectionString = builder.Configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' nije konfigurisan.");

        builder.Services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgres", tags: ["ready"]);

        return builder;
    }

    public static WebApplication UseObservability(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, exception) =>
                IsHealthCheckRequest(httpContext.Request.Path)
                    ? LogEventLevel.Verbose
                    : exception is not null || httpContext.Response.StatusCode >= 500
                        ? LogEventLevel.Error
                        : LogEventLevel.Information;
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false,
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
        });

        return app;
    }

    private static bool IsHealthCheckRequest(PathString path)
        => path.StartsWithSegments("/health");

    private static string GetServiceVersion()
        => typeof(Program).Assembly
               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
               .InformationalVersion
           ?? "unknown";
}
