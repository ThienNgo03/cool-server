using Mvvm;
using Navigation;

namespace Version1.Features.Authentication.SignIn;

public partial class ViewModel(
    Library.Token.Service tokenService,
    Library.Authentication.Interface authInterface,
    IAppNavigator appNavigator) : BaseViewModel(appNavigator)
{
    #region [ Fields ]

    private readonly Library.Token.Service tokenService = tokenService;
    private readonly Library.Authentication.Interface authInterface = authInterface;    
    #endregion

    #region [ UI ]

    public Form Form { get; init; } = new();

    [RelayCommand]
    async Task SignInAsync()
    {
        var isValid = Form.IsValid();
        if (!isValid)
        {
            await AppNavigator.ShowSnackbarAsync("Please fill in all fields");
            return;
        }

        var result = await authInterface.SignInAsync(new()
        {
            Account = Form.Account,
            Password = Form.Password
        });

        if(result is null || result.Token is null || string.IsNullOrEmpty(result.Token))
        {
            await AppNavigator.ShowSnackbarAsync("Sign in failed, please double check your account and password");
            return;
        };

        tokenService.SetToken(result.Token);

        //Set App.CurrentUser to easily access current user credentials

        await GoHomeAsync();
    }


    [RelayCommand]
    Task SignUpAsync() => AppNavigator.NavigateAsync(AppRoutes.SignUp);

    [RelayCommand]
    Task ForgotPasswordAsync() => AppNavigator.NavigateAsync(AppRoutes.ForgotPassword);

    Task GoHomeAsync() => AppNavigator.NavigateAsync(AppRoutes.Home);
    #endregion
}
