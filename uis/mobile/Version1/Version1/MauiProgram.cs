﻿using UI;
using Mvvm;
using Library;
using Navigation;
using Maui.FreakyEffects;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using Version1.Features.Authentication;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Version1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Damageplan.ttf", "Damageplan");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FontNames.FluentSystemIconsRegular);
            })
            .RegisterCore()
            .RegisterFeatures()
            .RegisterPages()
            .UseSkiaSharp();

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


        Library.Config locaHostConfig = new("https://localhost:7011");
        Library.Config devTunnelEnviroment = new("https://qs5xs4dh-7011.asse.devtunnels.ms");
        builder.Services.AddEndpoints(devTunnelEnviroment);
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

    static IServiceCollection AddPopup<TPopup, TViewModel>(this IServiceCollection services, string name)
        where TPopup : BasePopup where TViewModel : BaseViewModel
    {
        Routing.RegisterRoute(name, typeof(TPopup));
        return services;
    }
}
