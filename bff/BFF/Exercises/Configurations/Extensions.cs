namespace BFF.Exercises.Configurations;

public static class Extensions
{
    public static IServiceCollection AddExerciseConfigurations(this IServiceCollection services)
    {
        services.AddScoped<Detail.IMapper, Detail.Mapper>();
        services.AddScoped<IMapper, Mapper>();
        return services;
    }
}
