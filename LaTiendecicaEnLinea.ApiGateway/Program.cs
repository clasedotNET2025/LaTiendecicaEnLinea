using LaTiendecicaEnLinea.ApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configurar YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// HTTPS solo si está configurado
if (builder.Configuration.GetValue<bool>("EnableHttpsRedirection", true))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapReverseProxy();
app.MapControllers();

app.Run();