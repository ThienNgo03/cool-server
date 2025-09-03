using Syncfusion.Maui.Toolkit.NumericUpDown;

namespace Version1.Features.Exercises.Config;

public partial class Page : ContentPage
{
    private readonly ViewModel viewModel;
    public Page(ViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = this.viewModel = viewModel;

    }


    private void WeeklyItems_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.Chips.SelectionChangedEventArgs e)
    {
        var addedItem = e.AddedItem;
        var removedItem = e.RemovedItem;

        if (addedItem is WeeklyItem addedWeeklyItem)
        {
            viewModel.WorkoutTimeItems?.Add(new()
            {
                Id = addedWeeklyItem.Id,
                Content = addedWeeklyItem.Content,
                Time = new(6, 0, 0)
            });

            viewModel.SelectedDays.Add(addedWeeklyItem);
        }

        if (removedItem is WeeklyItem removedWeeklyItem)
        {
            viewModel.WorkoutTimeItems?.Remove(viewModel.WorkoutTimeItems?.FirstOrDefault(x => x.Id == removedWeeklyItem.Id));
            viewModel.SelectedDays.Remove(removedWeeklyItem);

            var setsToRemove = viewModel.TotalSets.Where(x => string.Equals(x.Day, removedWeeklyItem.Content, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var set in setsToRemove)
            {
                viewModel.TotalSets.Remove(set);
            }

            if (viewModel.SelectedDayForSet?.Content == removedWeeklyItem.Content)
            {
                viewModel.SelectedDayForSet = null; 
            }
        }
    }


    private void AddSetButton_Clicked(object sender, EventArgs e)
    {
        if (viewModel.SelectedDayForSet is null)
            return;

        var newSet = new SetConfigItem
        {
            Id = Guid.NewGuid().ToString(),
            Content = $"Set {viewModel.TotalSets.Count(x => string.Equals(x.Day, viewModel.SelectedDayForSet.Content, StringComparison.OrdinalIgnoreCase)) + 1}",
            Reps = 10,
            Day = viewModel.SelectedDayForSet.Content
        };

        viewModel.TotalSets.Add(newSet);
    }


    private void Remove_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Label removeLabel &&
            removeLabel.BindingContext is SetConfigItem setConfigItem)
        {
            var totalSetItem = viewModel.TotalSets.FirstOrDefault(x => x.Id == setConfigItem.Id);
            if (totalSetItem != null)
            {
                viewModel.TotalSets.Remove(totalSetItem);
            }

            viewModel.SummaryTotalReps = viewModel.TotalSets.Sum(x => x.Reps);
        }
    }

    private void RepsEntry_ValueChanged(object sender, Syncfusion.Maui.Toolkit.NumericEntry.NumericEntryValueChangedEventArgs e)
    {
        if (sender is SfNumericUpDown numericUpDown &&
        numericUpDown.BindingContext is SetConfigItem setConfigItem)
        {
            var totalSetItem = viewModel.TotalSets.FirstOrDefault(x => x.Id == setConfigItem.Id);
            if (totalSetItem != null)
            {
                totalSetItem.Reps = (int)e.NewValue;
            }
            viewModel.SummaryTotalReps = viewModel.TotalSets.Sum(x => x.Reps);
        }
    }
}