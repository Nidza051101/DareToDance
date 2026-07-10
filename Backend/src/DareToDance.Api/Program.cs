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
    app.MapControllers();

    app.Run();
}

