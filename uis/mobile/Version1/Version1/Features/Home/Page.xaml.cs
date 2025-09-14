namespace Version1.Features.Home;

public partial class Page : ContentPage
{
    #region [ Fields ]

    private readonly ViewModel viewModel;
    #endregion

    #region [ CTors ]

    public Page(ViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = this.viewModel = viewModel;
    }
    #endregion

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await this.viewModel.OnAppearingAsync();
    }
}