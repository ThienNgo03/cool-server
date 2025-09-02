namespace Version1.Features.Exercises.Detail;

public partial class Page : ContentPage
{
    private readonly ViewModel viewModel;
    public Page(ViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = this.viewModel = viewModel;

    }
}