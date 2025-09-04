namespace Version1.Features.Authentication.SignIn;

public partial class Page : ContentPage
{
    #region [ CTors ]

    public Page(ViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel; 
    }
    #endregion


    private async void SignInButton_Clicked(object sender, EventArgs e)
    {
        await AccountEntry.HideSoftInputAsync(CancellationToken.None);
        await PasswordEntry.HideSoftInputAsync(CancellationToken.None);
    }
}