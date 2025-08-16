using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Library.WeekPlans;

public static class Extensions
{
    public static void RegisterWeekPlans(this IServiceCollection services, bool isLocal)
    {
        services.AddTransient<Interface, Implementations.Version1.Implementation>();
        services.AddTransient<Implementations.Version1.RefitHttpClientHandler>();

        string baseUrl = isLocal
            ? "https://localhost:7011"
            : "";

        services.AddRefitClient<Implementations.Version1.IRefitInterface>()
                .ConfigurePrimaryHttpMessageHandler<Implementations.Version1.RefitHttpClientHandler>()
                .ConfigureHttpClient(x => x.BaseAddress = new Uri(baseUrl));
    }
}
