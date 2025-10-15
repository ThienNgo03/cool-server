namespace BFF.Exercises;

public static class Extensions
{
    public static IServiceCollection AddExercises(this IServiceCollection services)
    {
        services.AddScoped<All.IMapper, All.Mapper>();
        services.AddScoped<IMapper, Mapper>();
        return services;
    }
}
