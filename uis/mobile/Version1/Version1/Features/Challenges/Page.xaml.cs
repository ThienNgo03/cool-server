namespace Version1.Features.Challenges;

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
}