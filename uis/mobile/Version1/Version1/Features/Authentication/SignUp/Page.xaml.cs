namespace Version1.Features.Authentication.SignUp;

public partial class Page : ContentPage
{
	public Page(ViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
    }
}