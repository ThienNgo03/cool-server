using Core.Authentication;
using Core.Exercises;
using Core.Exercises.Configurations;
using Core.User;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class Extensions
{
    public static IServiceCollection AddBff(this IServiceCollection services, Config config)
    {
        services.AddSingleton<Token.Service>();

        services.RegisterAuthentication(config);
        services.RegisterExerciseConfigurations(config);
        services.RegisterExercises(config);
        services.RegisterUsers(config);
        return services;
    }
}
