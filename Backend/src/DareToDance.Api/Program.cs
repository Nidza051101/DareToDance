using DareToDance.Api.Common.Endpoints;
using DareToDance.Api.Common.Extensions;
using DareToDance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
    }

    builder.Services.AddProblemDetails();
    builder.Services.AddControllers();
    builder.Services.AddPresentation();
    builder.Services.AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.ApplyMigrations();
    }

    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapEndpoints();

    app.Run();
}
