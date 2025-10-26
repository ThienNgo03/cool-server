using Microsoft.Extensions.Options;
using Refit;
using Journal.Databases;
using Journal.Databases.OpenSearch;

namespace Journal.Exercises.Get.SuperSearch;

public static class Extensions
{
    public static void RegisterExercisesSuperSearch(this IServiceCollection services)
    {
        services.AddTransient<RefitHttpClientHandler>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<OpenSearchConfig>>().Value;
            return new RefitHttpClientHandler(
                config.Username,
                config.Password,
                config.SkipCertificateValidation
            );
        });

        services.AddRefitClient<IRefitInterface>()
                .ConfigurePrimaryHttpMessageHandler<RefitHttpClientHandler>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var config = sp.GetRequiredService<IOptions<OpenSearchConfig>>().Value;
                    var builder = new ConnectionStringBuilder()
                        .WithHost(config.Host)
                        .WithPort(config.Port);

                    if (config.EnableSsl)
                        builder.WithSsl();

                    client.BaseAddress = new Uri(builder.Build());
                });
    }
}