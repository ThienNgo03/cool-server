using Azure.Storage.Blobs;

namespace Journal.Files;

public static class Extensions
{
    public static IServiceCollection AddFile(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = "UseDevelopmentStorage=true";
        var containerName = "container1";

        services.AddSingleton(new BlobContainerClient(connectionString, containerName));

        return services;
    }
}
