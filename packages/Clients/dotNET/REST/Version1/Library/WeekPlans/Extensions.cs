using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Library.WeekPlans;

public static class Extensions
{
    public static void RegisterWeekPlans(this IServiceCollection services, Config config)
    {
        services.AddTransient<Implementations.Version1.RefitHttpClientHandler>();
        services.AddTransient<Interface, Implementations.Version1.Implementation>();

        string baseUrl = config.Url;

        services.AddRefitClient<Implementations.Version1.IRefitInterface>()
                .ConfigurePrimaryHttpMessageHandler<Implementations.Version1.RefitHttpClientHandler>()
                .ConfigureHttpClient(x => x.BaseAddress = new Uri(baseUrl));
    }
}
