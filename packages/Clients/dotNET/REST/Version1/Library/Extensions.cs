using Library.Competitions;
using Library.Exercises;
using Library.SoloPools;
using Library.TeamPools;
using Microsoft.Extensions.DependencyInjection;

namespace Library;

public static class Extensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, bool isLocal)
    {
        services.RegisterExercises(isLocal);
        services.RegisterCompetitions(isLocal);
        services.RegisterSoloPools(isLocal);
        services.RegisterTeamPools(isLocal);
        return services;
    }
}
