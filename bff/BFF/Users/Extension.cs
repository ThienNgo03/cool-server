namespace BFF.Users;

public static class Extensions
{
    public static IServiceCollection AddUsers(this IServiceCollection services)
    {
        services.AddScoped<All.IMapper, All.Mapper>();
        services.AddScoped<IMapper, Mapper>();
        return services;
    }
}
