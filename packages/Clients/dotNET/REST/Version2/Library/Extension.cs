using Library.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Library;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ApiContext to the dependency injection container.
    /// Similar to AddDbContext in Entity Framework Core.
    /// </summary>
    public static IServiceCollection AddApiContext<TContext>(
        this IServiceCollection services,
        Action<ApiContextOptions> configureOptions)
        where TContext : ApiContext
    {
        var options = new ApiContextOptions();
        configureOptions(options);

        services.AddSingleton(options);
        services.AddScoped<TContext>(provider =>
        {
            var contextOptions = provider.GetRequiredService<ApiContextOptions>();

            // If using named HttpClient, get it from factory
            if (!string.IsNullOrEmpty(contextOptions.HttpClientName))
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(contextOptions.HttpClientName);
                return (TContext)Activator.CreateInstance(typeof(TContext), contextOptions, httpClient)!;
            }

            // Otherwise use default constructor
            return (TContext)Activator.CreateInstance(typeof(TContext), contextOptions)!;
        });

        return services;
    }

    /// <summary>
    /// Adds the default ApiContext to the DI container.
    /// </summary>
    public static IServiceCollection AddApiContext(
        this IServiceCollection services,
        Action<ApiContextOptions> configureOptions)
    {
        return services.AddApiContext<ApiContext>(configureOptions);
    }
}