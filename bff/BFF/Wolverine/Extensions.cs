using Wolverine;

namespace BFF.Wolverine;

public static class Extensions
{
    public static IServiceCollection AddWolverine(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWolverine(opts =>
        {
            //Message 
            opts.PublishMessage<Messages.DELETE.Messager.Message>().ToLocalQueue("message-delete");
            opts.PublishMessage<Messages.PUT.Messager.Message>().ToLocalQueue("message-update");
            opts.PublishMessage<Messages.POST.Messager.Message>().ToLocalQueue("message-create");
        });
        return services;
    }
}
