namespace Version1.Features.Authentication.ForgotPassword;

public partial class Page : ContentPage
{
    #region [ CTors ]

    public Page(ViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel; 
    }
    #endregion
}