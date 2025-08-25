using Mvvm;
using Navigation;

namespace Version1.Features.Exercises;

public partial class ViewModel(
    IAppNavigator appNavigator,
    Library.Workouts.Interface workouts,
    Library.Exercises.Interface exercises) : BaseViewModel(appNavigator)
{
    #region [ Fields ]

    private readonly Library.Workouts.Interface workouts = workouts;
    private readonly Library.Exercises.Interface exercises = exercises;
    #endregion

    #region [ UI ]

    [ObservableProperty]
    ObservableCollection<ContentViews.Card.Model> items = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        var exercises = await LoadExercisesAsync(); 
        foreach (var item in exercises)
        {
            Items.Add(new ContentViews.Card.Model()
            {
                Id = item.Id.ToString(),
                Title = item.Name,
                Description = item.Description,
                IconUrl = "dotnet_bot.png"
            });
        }
    }
    #endregion

    #region [ Utils ]

    async Task<ICollection<Library.Exercises.Model>> LoadExercisesAsync()
    {
        var response = await this.exercises.AllAsync();

        if(response == null || response.Data == null || response.Data.Items == null || response.Data.Items.Count == 0)
            return new ObservableCollection<Library.Exercises.Model>();

        return response.Data.Items;
    }

    async Task<ICollection<Library.Workouts.Model>> LoadWorkoutsAsync(
        Guid? userId, 
        bool isIncludeWeekPlans)
    {
        var response = await this.workouts.AllAsync(new() 
        { 
            UserId = userId,
            IsIncludeWeekPlans = isIncludeWeekPlans
        });

        if (response == null || response.Data == null || response.Data.Items == null || response.Data.Items.Count == 0)
            return new ObservableCollection<Library.Workouts.Model>();

        return response.Data.Items;
    }



    async Task<ICollection<ContentViews.Chip.Model>> LoadMusclesAsync()
    {
        var response = await exercises.AllAsync();
        ObservableCollection<ContentViews.Chip.Model> items = new();
        foreach (var item in response.Data?.Items)
        {
            items.Add(new ContentViews.Chip.Model()
            {
                Id = item.Id.ToString(),
                Text = item.Name,
                IsSelected = false
            });
        }
        return items;
    }
    #endregion
}
