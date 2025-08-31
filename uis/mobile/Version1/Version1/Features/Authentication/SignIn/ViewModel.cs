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
        //var isValid = Form.IsValid();
        //if (!isValid)
        //    return;

        // Authenticate
        //var result = await authInterface.SignInAsync(new()
        //{
        //    Account = Form.Account,
        //    Password = Form.Password
        //});

        //tokenService.SetToken(result?.Token ?? string.Empty);

        //if (result is null || string.IsNullOrWhiteSpace(result.Token))
        //    return;

        await GoHomeAsync();
    }


    [RelayCommand]
    Task SignUpAsync() => AppNavigator.NavigateAsync(AppRoutes.SignUp);

    [RelayCommand]
    Task ForgotPasswordAsync() => AppNavigator.NavigateAsync(AppRoutes.ForgotPassword);

    Task GoHomeAsync() => AppNavigator.NavigateAsync(AppRoutes.Home);
    #endregion
}
