using Mvvm;
using Navigation;

namespace Version1.Features.Profile;

public partial class ViewModel(IAppNavigator appNavigator) : BaseViewModel(appNavigator)
{
    [RelayCommand]
    async Task Logout()
    {
await AppNavigator.NavigateAsync(AppRoutes.SignIn);
    }
}
