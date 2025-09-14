using Microsoft.Extensions.DependencyInjection;
using Refit;
namespace Library.Files.Identifiers;

public static class Extensions
{
    public static void RegisterFilesIdentifiers(this IServiceCollection services, Config config)
    {
        services.AddTransient<Implementations.Version1.RefitHttpClientHandler>();
        services.AddTransient<Interface, Implementations.Version1.Implementation>();

        string baseUrl = config.Url;

        services.AddRefitClient<Implementations.Version1.IRefitInterface>()
                .ConfigurePrimaryHttpMessageHandler<Implementations.Version1.RefitHttpClientHandler>()
                .ConfigureHttpClient(x => x.BaseAddress = new Uri(baseUrl));
    }
}