
var builder = WebApplication.CreateBuilder(args);
{
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
    }
    
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
