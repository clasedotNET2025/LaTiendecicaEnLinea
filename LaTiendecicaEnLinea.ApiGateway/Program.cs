// Program.cs del Gateway - VERSIÓN SIMPLIFICADA Y FUNCIONAL
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Configure Reverse Proxy SIN transforms personalizados
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCors", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors");
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapReverseProxy();

// Simple health check
app.MapGet("/", () => "API Gateway is running");
app.MapGet("/health", () => new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Services = new[]
    {
        new { Service = "Identity", Route = "/api", Status = "Connected" },
        new { Service = "Catalog", Route = "/catalog", Status = "Connected" },
        new { Service = "Orders", Route = "/orders", Status = "Connected" }
    }
});

app.Run();