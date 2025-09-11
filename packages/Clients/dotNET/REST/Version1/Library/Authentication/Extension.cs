using Google.Protobuf.WellKnownTypes;
using Journal.Beta.Authentication.Login;
using Journal.Beta.Authentication.Register;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Library.Authentication;

public static class Extensions
{
    public static void RegisterAuthentication(this IServiceCollection services, Config config)
    {
        services.AddTransient<Implementations.Version1.RefitHttpClientHandler>();
        //services.AddTransient<Interface, Implementations.Version1.Implementation>();

        string baseUrl = config.Url;

        services.AddRefitClient<Implementations.Version1.IRefitInterface>()
                .ConfigurePrimaryHttpMessageHandler<Implementations.Version1.RefitHttpClientHandler>()
                .ConfigureHttpClient(x => x.BaseAddress = new Uri(baseUrl));

        services.AddGrpcClient<LoginMethod.LoginMethodClient>(o => o.Address = new Uri(baseUrl));
        services.AddGrpcClient<RegisterMethod.RegisterMethodClient>(o => o.Address = new Uri(baseUrl));
        services.AddTransient<Interface,Implementations.Version2.Implementation>();

    }
}

