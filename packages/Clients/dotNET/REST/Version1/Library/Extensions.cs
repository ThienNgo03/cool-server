using Library.Competitions;
using Library.Exercises;
using Library.WeekPlans;
using Library.Workouts;
using Library.SoloPools;
using Library.TeamPools;
using Library.WorkoutLogs;
using Library.MeetUps;
using Library.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Library.WeekPlanSets;
using Library.WorkoutLogSets;
using Library.Muscles;

namespace Library;

public static class Extensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Config config)
    {
        services.AddSingleton<Token.Service>();

        services.RegisterAuthentication(config);
        services.RegisterExercises(config);
        services.RegisterWorkouts(config);
        services.RegisterWeekPlans(config);
        services.RegisterCompetitions(config);
        services.RegisterSoloPools(config);
        services.RegisterTeamPools(config);
        services.RegisterWorkoutLogs(config);
        services.RegisterMeetUps(config);
        services.RegisterWeekPlanSets(config);
        services.RegisterWorkoutLogSets(config);
        services.RegisterMuscles(config);
        return services;
    }
}
