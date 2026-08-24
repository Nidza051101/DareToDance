using DareToDance.Api.Common.Extensions;
using DareToDance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
    }

    builder.AddObservability();

    builder.Services.AddProblemDetails();
    builder.Services.AddControllers();
    builder.Services.AddPresentation();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
    builder.Services.AddApiAuthentication(builder.Configuration);
    builder.Services.AddApiRateLimiting(builder.Configuration);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddApiDocumentation();
    }
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.ApplyMigrations();
        app.UseApiDocumentation();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseExceptionHandler();
    app.UseObservability();
    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}

public partial class Program { }