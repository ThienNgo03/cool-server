
namespace Journal.Journeys;

public static class Extensions
{
    public static IServiceCollection AddJourneys(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<Get.Interface, Get.Implementations.Version1.Implementation>();
        return services;

        
    }
}
