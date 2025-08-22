using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mvvm;
using Navigation;
using Syncfusion.Maui.Toolkit.Hosting;
using Version1.Features.Authentication;
using Library;

namespace Version1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .RegisterCore()
            .RegisterFeatures()
            .RegisterPages();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    static MauiAppBuilder RegisterCore(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IAppInfo>(AppInfo.Current);
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);
        builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);

        builder.Services.AddSingleton<IAppNavigator, AppNavigator>();
        builder.Services.AddEndpoints(true, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwYjAzYjVjMi0yMzAzLTRmODMtOGQ5Ny01ODk1ZDVmN2M0NTYiLCJlbWFpbCI6InN5c3RlbXRlc3RlckBqb3VybmFsLmNvbSIsImp0aSI6ImFjZWU3ZDgzLWY4NzUtNGIzMi05ZjIyLWM1ZTcyOTg4MWFiNiIsImV4cCI6MTc1NTg4MTIyMCwiaXNzIjoiQXV0aENvb2xTZXJ2ZXIiLCJhdWQiOiJBdXRoQ29vbENsaWVudCJ9.LZXfumO0ThL28agjlzZnWJwXY_XGtEE-RsTxI3hQaOE");
        return builder;
    }

    static MauiAppBuilder RegisterFeatures(this MauiAppBuilder builder)
    {
        builder.RegisterAuthentication();
        return builder;
    }

    static MauiAppBuilder RegisterPages(this MauiAppBuilder builder)
    {
        var pageTypes = typeof(MauiProgram).Assembly
                            .GetTypes()
                            .Where(x => !x.IsAbstract &&
                                    x != typeof(BasePage) &&
                                    x.IsAssignableTo(typeof(BasePage)));
        foreach (var pageType in pageTypes)
        {
            builder.Services.AddTransient(pageType);
        }

        var viewModelTypes = typeof(MauiProgram).Assembly
                            .GetTypes()
                            .Where(
                                x => !x.IsAbstract &&
                                    x != typeof(BaseViewModel) &&
                                    x != typeof(NavigationAwareBaseViewModel) &&
                                    (x.IsAssignableTo(typeof(BaseViewModel)) ||
                                     x.IsAssignableTo(typeof(NavigationAwareBaseViewModel)))
                            )
                            .ToList();
        foreach (var vmType in viewModelTypes)
        {
            builder.Services.AddTransient(vmType);
        }

        return builder;
    }

    static IServiceCollection AddPage<TPage, TViewModel>(this IServiceCollection services)
        where TPage : BasePage where TViewModel : BaseViewModel
    {
        services.AddTransient<TPage>();
        services.AddTransient<TViewModel>();
        return services;
    }

    static IServiceCollection AddPopup<TPopup, TViewModel>(this IServiceCollection services, string name)
        where TPopup : BasePopup where TViewModel : BaseViewModel
    {
        Routing.RegisterRoute(name, typeof(TPopup));
        return services;
    }
}
