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

    private void AddSet(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var id = button.CommandParameter as string;
        var item = this.viewModel.Items.FirstOrDefault(x => x.Id == id);

        if(item == null)
            return;

        int indexOfNextSet = item.Sets.Count;

        item.Sets.Add(new()
        {
            Id = Guid.NewGuid(),
            Text = $"Set {indexOfNextSet + 1}",
            Value = 10
        });
    }

    private void IncreaseSet(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var set = button.CommandParameter as ContentViews.Card.Set;

        if (set == null)
            return;

        var setFromViewModel = viewModel.Items.SelectMany(x => x.Sets).FirstOrDefault(x => x.Id == set.Id);
        if(setFromViewModel == null)
            return;

        setFromViewModel.Value++;
    }

    private void DecreaseSet(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var set = button.CommandParameter as ContentViews.Card.Set;
        if (set == null || set.Value <= 0)
            return;

        var setFromViewModel = viewModel.Items.SelectMany(x => x.Sets).FirstOrDefault(x => x.Id == set.Id);
        if (setFromViewModel == null)
            return;

        setFromViewModel.Value--;
    }


    private async void SelectButton_Clicked(object sender, EventArgs e)
    {
        var button = (SfButton)sender;
        button.Background = button.IsChecked ? Brush.Aqua : Brush.Brown;

        var id = button.CommandParameter as string;
        await this.viewModel.UpdateCardAsync(id, button.IsChecked);
    }

    private async void DayOfWeekButton_Clicked(object sender, EventArgs e)
    {
        var button = (SfButton)sender;
        button.Background = button.IsChecked ? Brush.Aqua : Brush.Brown;

        var dayOfWeek = (DayOfWeek)button.CommandParameter;
    }
}