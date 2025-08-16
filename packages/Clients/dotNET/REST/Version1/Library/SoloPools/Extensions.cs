using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Library.SoloPools;

public static class Extensions
{
    public static void RegisterSoloPools(this IServiceCollection services, bool isLocal)
    {
        services.AddTransient<Implementations.Version1.RefitHttpClientHandler>();
        services.AddTransient<Interface, Implementations.Version1.Implementation>();

        string baseUrl = isLocal
            ? "https://localhost:7011"
            : "";

        services.AddRefitClient<Implementations.Version1.IRefitInterface>()
                .ConfigurePrimaryHttpMessageHandler<Implementations.Version1.RefitHttpClientHandler>()
                .ConfigureHttpClient(x => x.BaseAddress = new Uri(baseUrl));
    }
}
