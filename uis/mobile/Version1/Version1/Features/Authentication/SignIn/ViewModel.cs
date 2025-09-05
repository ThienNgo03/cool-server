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

    [ObservableProperty]
    bool isLoading = false;

    partial void OnIsLoadingChanged(bool value)
    {
        IsAccountEntryEnable = !value;
        IsPasswordEntryEnable = !value;
        IsLoginButtonEnable = !value;
        IsCreateAccountButtonEnable = !value;
        IsForgotPasswordButtonEnable = !value;
    }

    [ObservableProperty]
    bool isAccountEntryEnable = true;

    [ObservableProperty]
    bool isPasswordEntryEnable = true;

    [ObservableProperty]
    bool isLoginButtonEnable = true;

    [ObservableProperty]
    bool isCreateAccountButtonEnable = true;

    [ObservableProperty]
    bool isForgotPasswordButtonEnable = true;

    public Form Form { get; init; } = new();

    [RelayCommand]
    async Task SignInAsync()
    {
        IsLoading = true;
        var isValid = Form.IsValid();
        if (!isValid)
        {
            await AppNavigator.ShowSnackbarAsync("Please fill in all fields");
            IsLoading = false;
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
            IsLoading = false;
            return;
        };

        tokenService.SetToken(result.Token);
        IsLoading = false;
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
