using Library.Exercises;
using Microsoft.Extensions.DependencyInjection;

namespace Library;

public static class Extensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, bool isLocal)
    {
        services.RegisterExercises(isLocal);
        return services;
    }
}
