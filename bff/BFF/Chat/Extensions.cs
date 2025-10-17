namespace BFF.Chat;

public static class Extensions
{
    public static IServiceCollection AddChat(this IServiceCollection services)
    {
        services.AddScoped<Messages.IMapper, Messages.Mapper>();
        services.AddScoped<IMapper, Mapper>();
        return services;
    }
}
