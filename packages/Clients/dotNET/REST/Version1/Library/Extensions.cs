using Library.Competitions;
using Library.Exercises;
using Library.WeekPlans;
using Library.Workouts;
using Library.SoloPools;
using Library.TeamPools;
using Library.WorkoutLogs;
using Microsoft.Extensions.DependencyInjection;

namespace Library;

public static class Extensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, bool isLocal, string token)
    {
        Models.Authentication.Model authentication = new() { BearerToken = token };
        services.AddSingleton(authentication);

        services.RegisterExercises(isLocal);
        services.RegisterWorkouts(isLocal);
        services.RegisterWeekPlans(isLocal);
        services.RegisterCompetitions(isLocal);
        services.RegisterSoloPools(isLocal);
        services.RegisterTeamPools(isLocal);
        services.RegisterWorkoutLogs(isLocal);
        return services;
    }
}
