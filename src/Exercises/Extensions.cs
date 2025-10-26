using Journal.Databases;
using Journal.Exercises.Get;
using Journal.Exercises.Get.SuperSearch;

namespace Journal.Exercises;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.RegisterExercisesSuperSearch();
        return services;
    }
}
