using Syncfusion.Maui.Toolkit.Buttons;

namespace Version1.Features.Exercises;

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
        await viewModel.LoadAsync();
    }

    private async void Detail_Clicked(object sender, EventArgs e)
    {
        var button = (SfButton)sender;
        var exerciseId = (string)button.CommandParameter;
        if (exerciseId is null)
            return;

        await viewModel.NavigateAsync(AppRoutes.ExerciseDetail, new Dictionary<string, object>
                    {
                        { "Id", exerciseId }
                    });
    }

    private async void ConfigButton_Clicked(object sender, EventArgs e)
    {
        var button = (ImageButton)sender;
        var exerciseId = (string)button.CommandParameter;
        if (exerciseId is null)
            return;

        await viewModel.NavigateAsync(AppRoutes.ExerciseConfig, new Dictionary<string, object>
                    {
                        { "Id", exerciseId }
                    });
    }
}