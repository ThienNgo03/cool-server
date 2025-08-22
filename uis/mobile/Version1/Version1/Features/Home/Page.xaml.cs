namespace Version1.Features.Home;

public partial class Page : ContentPage
{
    public Page(ViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}