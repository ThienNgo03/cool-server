using Syncfusion.Maui.Toolkit.Buttons;

namespace Version1.Features.Exercises;

public partial class Page : ContentPage
{
    private readonly ViewModel viewModel;
    public Page(ViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = this.viewModel = viewModel;

        viewModel.LoadAsync();
    }


    private async void SelectButton_Clicked(object sender, EventArgs e)
    {
        var button = (SfButton)sender;
        //button.Text = button.IsChecked ? "✅ Selected" : "🔘 Select";
        //button.BackgroundColor = button.IsChecked ? Color.FromHex("#171717") : Color.FromHex("#27354a");
        ////button.IsChecked = !button.IsChecked;

        var id = button.CommandParameter as string;
        await this.viewModel.UpdateCardAsync(id, button.IsChecked);
    }
}