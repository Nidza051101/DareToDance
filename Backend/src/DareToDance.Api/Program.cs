using DareToDance.Application;
using DareToDance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
    }

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration);
    
    builder.Services.AddControllers();
}

var app = builder.Build(); 
{
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}

// makes the implicit Program class visible to WebApplicationFactory in integration tests
public partial class Program;

