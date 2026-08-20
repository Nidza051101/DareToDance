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
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApiAuthentication(builder.Configuration);

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

    app.UseExceptionHandler();
    app.UseObservability();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}

public partial class Program { }
