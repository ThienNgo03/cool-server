using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Library.Authentication;

public static class Extensions
{
    public static void RegisterAuthentication(this IServiceCollection services, Config config)
    {
        services.AddTransient<Implementations.Version1.RefitHttpClientHandler>();

        string baseUrl = config.Url;
        string secretKey = config.SecretKey;

        services.AddRefitClient<Implementations.Version1.IRefitInterface>()
                .ConfigurePrimaryHttpMessageHandler<Implementations.Version1.RefitHttpClientHandler>()
                .ConfigureHttpClient(x => x.BaseAddress = new Uri(baseUrl));

        services.AddTransient<Interface, Implementations.Version1.Implementation>();

    }
}

