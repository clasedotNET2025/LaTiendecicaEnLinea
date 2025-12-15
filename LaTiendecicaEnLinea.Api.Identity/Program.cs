using Asp.Versioning;
using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Data;
using LaTiendecicaEnLinea.Api.Identity.Services;
using LaTiendecicaEnLinea.Identity.Extensions;
using LaTiendecicaEnLinea.Shared.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddUserSecrets(typeof(Program).Assembly, true);

builder.Services.AddControllers();

// Configurar OpenAPI con seguridad JWT
builder.Services.AddOpenApi("v1", options =>
{
    options.ConfigureDocumentInfo(
        "La Tiendecica En Línea - Identity API V1",
        "v1",
        "Authentication API using Controllers with JWT Bearer authentication");
    options.AddJwtBearerSecurity();
    options.FilterByApiVersion("v1");
});

builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("rabbitmq");

        if (!string.IsNullOrEmpty(connectionString))
        {
            cfg.Host(new Uri(connectionString));
        }

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IPublishEndpoint>(provider =>
    provider.GetRequiredService<IBus>()
);

// Configurar versionamiento de API
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

// Validadores FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Base de datos PostgreSQL
builder.AddNpgsqlDbContext<MyAppContext>("identity");

// Configurar ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false; // Cambiar a true en producción
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<MyAppContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IUserService, UserService>();

// JWT BEARER AUTHENTICATION (MÉTODO DE EXTENSIÓN)
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "La Tiendecica Identity API V1");
    });

    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<MyAppContext>();
    await context.Database.MigrateAsync();

    // CREAR ROLES INICIALES
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in Roles.GetAll())
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"Rol '{role}' creado exitosamente.");
        }
        else
        {
            Console.WriteLine($"Rol '{role}' ya existe.");
        }
    }

    // CREAR USUARIO ADMIN POR DEFECTO
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var adminEmail = builder.Configuration["AdminCredentials:Email"]
    ?? throw new InvalidOperationException("Admin email not configured");
    var adminPassword = builder.Configuration["AdminCredentials:Password"]
        ?? throw new InvalidOperationException("Admin password not configured");

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);

        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("Usuario admin creado exitosamente:");
            Console.WriteLine($"  Email: {adminEmail}");
            Console.WriteLine($"  Password: {adminPassword}");
            Console.WriteLine($"  Rol: {Roles.Admin} asignado");
        }
        else
        {
            Console.WriteLine("Error al crear usuario admin:");
            foreach (var error in createResult.Errors)
            {
                Console.WriteLine($"  - {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine($"Usuario admin '{adminEmail}' ya existe.");

        var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");
        if (!isInRole)
        {
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            Console.WriteLine($"Rol {Roles.Admin} asignado al usuario existente.");
        }
        else
        {
            Console.WriteLine($"El usuario ya tiene rol {Roles.Admin}.");
        }
    }
}

app.UseHttpsRedirection();


// MIDDLEWARE DE AUTENTICACIÓN (IMPORTANTE)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();