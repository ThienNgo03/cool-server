
using Mvvm;
using Navigation;

namespace Version1.Features.Subscriptions;

public partial class ViewModel(
    IAppNavigator appNavigator,
    Core.Exercises.Interface exercises ) : BaseViewModel(appNavigator)
{

    [ObservableProperty]
    ObservableCollection<AppUsage> appUsages = new ObservableCollection<AppUsage>
    {
        new AppUsage { AppName = "App A", UsagePercent = 45 },
        new AppUsage { AppName = "App B", UsagePercent = 25.0 },
        new AppUsage { AppName = "App C", UsagePercent = 10.0 },
        new AppUsage { AppName = "App D", UsagePercent = 10 },
        new AppUsage { AppName = "App E", UsagePercent = 2.0 },
        new AppUsage { AppName = "App F", UsagePercent = 2.0 },
        new AppUsage { AppName = "App G", UsagePercent = 2.0 },
        new AppUsage { AppName = "App H", UsagePercent = 2.0 },
        new AppUsage { AppName = "App I", UsagePercent = 2.0 },
    };
}


public partial class AppUsage : ObservableObject
{
    [ObservableProperty]
    string appName;

    [ObservableProperty]
    double usagePercent;
}
