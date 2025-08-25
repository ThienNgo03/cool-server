namespace Version1.Features.Exercises;

public partial class Page : ContentPage
{
    public Page(ViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        viewModel.LoadAsync();
    }

    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
    }
}