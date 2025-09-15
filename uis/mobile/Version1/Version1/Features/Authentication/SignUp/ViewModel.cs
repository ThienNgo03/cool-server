using Mvvm;
using Navigation;

namespace Version1.Features.Authentication.SignUp;

public partial class ViewModel(IAppNavigator appNavigator,
    Library.Users.Interface usersBiz,
    Library.Authentication.Interface authBiz) : BaseViewModel(appNavigator)
{
    #region [ Fields ]

    private readonly Library.Users.Interface usersBiz = usersBiz;
    private readonly Library.Authentication.Interface authBiz = authBiz;
    #endregion

    #region [ UI ]
    [ObservableProperty]
    ImageSource avatarSource;

    [RelayCommand]
    public async Task<FileResult> PickAndShow(PickOptions options)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    // Set the AvatarSource directly from the file path
                    AvatarSource = ImageSource.FromFile(result.FullPath);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            // Handle exceptions appropriately
        }

        return null;
    }
    public Form Form { get; init; } = new();

    [RelayCommand]
    async Task SignUpAsync()
    {
        IsFuckingBusy = true;
        var isValid = Form.IsValid();
        if (!isValid)
        {
            await AppNavigator.ShowSnackbarAsync("Please fill in all fields");
            IsFuckingBusy = false;
            return;
        }
        await authBiz.RegisterAsync(new()
        {
            FirstName=Form.FirstName,
            LastName=Form.LastName,
            UserName = $"{Form.FirstName}{Form.LastName}",
            Email = Form.Email,
            Password = Form.Password,
            ConfirmPassword = Form.ConfirmPassword,
            PhoneNumber = Form.PhoneNumber,
        });
        IsFuckingBusy = false;
        await GoHomeAsync();
    }

    Task GoHomeAsync() => AppNavigator.NavigateAsync(AppRoutes.Home);

    #endregion
}
