using Mvvm;
using Navigation;

namespace Version1.Features.Authentication.SignIn;

public partial class ViewModel(
    Library.Users.Interface usersBiz,
    Library.Token.Service tokenService,
    Library.Authentication.Interface authBiz,
    Core.Token.Service coreToken,
    IAppNavigator appNavigator) : BaseViewModel(appNavigator)
{
    #region [ Fields ]

    private readonly Library.Users.Interface usersBiz = usersBiz;
    private readonly Library.Token.Service tokenService = tokenService;
    private readonly Library.Authentication.Interface authBiz = authBiz;

    private readonly Core.Token.Service coreToken = coreToken;
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

        var result = await authBiz.SignInAsync(new()
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
        coreToken.SetToken(result.Token);

        var currentUserInfo = await usersBiz.AllAsync(new() { IsSelf = true });

        if (currentUserInfo is null || currentUserInfo is null || currentUserInfo.Items is null || !currentUserInfo.Items.Any())
        {
            await AppNavigator.ShowSnackbarAsync("Failed to retrieve user information");
            IsLoading = false;
            return;
        }

        if (MyApp is null)
            return;

        MyApp.SetCurrentUser(
            id: currentUserInfo.Items.First().Id,
            name: currentUserInfo.Items.First().Name,
            age: 0,
            avatarUrl: currentUserInfo.Items.First().ProfilePicture,
            bio: string.Empty,
            token: result.Token
        );

        IsLoading = false;

        await GoHomeAsync();
    }


    [RelayCommand]
    Task SignUpAsync() => AppNavigator.NavigateAsync(AppRoutes.SignUp);

    [RelayCommand]
    Task ForgotPasswordAsync() => AppNavigator.NavigateAsync(AppRoutes.ForgotPassword);

    Task GoHomeAsync() => AppNavigator.NavigateAsync(AppRoutes.Home);
    #endregion
}
