using Mvvm;
using Navigation;

namespace Version1.Features.Authentication.SignIn;

public partial class ViewModel(IAppNavigator appNavigator) : BaseViewModel(appNavigator)
{
    #region [ UI ]

    public Form Form { get; init; } = new();

    [RelayCommand]
    Task SignInAsync()
    {
        var isValid = Form.IsValid();

        if (!isValid)
        {
            return Task.CompletedTask;
        }

        //appSettingsService.SetAccessTokenAsync(
        //    Convert.ToBase64String(
        //        System.Text.Encoding.UTF8.GetBytes(
        //            $"{Form.UserName}:{Form.Password}"
        //        )
        //    )
        //);

        return GoHomeAsync();
    }


    [RelayCommand]
    Task SignUpAsync() => AppNavigator.NavigateAsync(AppRoutes.SignUp);

    [RelayCommand]
    Task ForgotPasswordAsync() => AppNavigator.NavigateAsync(AppRoutes.ForgotPassword);

    Task GoHomeAsync() => AppNavigator.NavigateAsync(AppRoutes.Home);
    #endregion
}
