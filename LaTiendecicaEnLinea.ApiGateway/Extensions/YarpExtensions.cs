// LaTiendecicaEnLinea.ApiGateway/Extensions/YarpExtensions.cs
using Microsoft.AspNetCore.HttpOverrides;

namespace LaTiendecicaEnLinea.ApiGateway.Extensions;

public static class YarpExtensions
{
    public static IServiceCollection AddYarpReverseProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                      ForwardedHeaders.XForwardedProto |
                                      ForwardedHeaders.XForwardedHost;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        return services;
    }

    public static IServiceCollection AddGatewayCors(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        return services;
    }
}