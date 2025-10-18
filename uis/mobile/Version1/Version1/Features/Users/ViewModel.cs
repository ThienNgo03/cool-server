using Mvvm;
using Navigation;

namespace Version1.Features.Users;

public partial class ViewModel(
    IAppNavigator appNavigator,
    Core.User.Interface context) : BaseViewModel(appNavigator)
{
    #region [ Fields ]

    private readonly Core.User.Interface context = context;
    #endregion

    #region [ UI ]

    [ObservableProperty]
    ObservableCollection<string> tabs = new()
    {
        "All Users",
        "Lobby"
    };

    [ObservableProperty]
    string selectedTab = "All Users";

    [RelayCommand]
    public async Task LoadAsync()
    { }
    #endregion
}
