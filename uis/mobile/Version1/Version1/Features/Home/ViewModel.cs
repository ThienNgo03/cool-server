using Mvvm;
using Navigation;
using Library.Queryable.Include.Extensions;

namespace Version1.Features.Home;

public partial class ViewModel(IAppNavigator appNavigator, Library.Workouts.Interface workouts) : BaseViewModel(appNavigator)
{
    
    private readonly Library.Workouts.Interface workouts = workouts;

    [ObservableProperty]
    ObservableCollection<ContentViews.CarouselItem.Model> items = new();

    public override async Task OnAppearingAsync()
    {
        await base.OnAppearingAsync();
        IsFuckingBusy = true;
        await Task.Delay(2000); 

        var response = await workouts
            .Include(x => x.Exercise)
                .ThenInclude(x => x.Muscles)
            .Include(x => x.WeekPlans)
                .ThenInclude(x => x.WeekPlanSets)
            .AllAsync<Library.Workouts.All.Parameters>(new() 
            {
                UserId = MyApp?.CurrentUser?.Id
            });

        if (response.Data?.Items == null)
        {
            IsFuckingBusy = false;
            return;
        }

        var serverData = new List<ContentViews.CarouselItem.Model>();
        var dateOfWeek = DateTime.Today.DayOfWeek.ToString();
        foreach (var workout in response.Data.Items)
        {
            var weekPlans = workout.WeekPlans?.Where(wp => wp.DateOfWeek == dateOfWeek).ToList();
            if (weekPlans != null && weekPlans.Any())
            {
                var carouselItem = new ContentViews.CarouselItem.Model { Id = workout.Id };
                foreach (var weekPlan in weekPlans)
                {
                    carouselItem.Time = weekPlan.Time.ToString(@"hh\:mm");
                    if (weekPlan.WeekPlanSets != null)
                    {
                        var setIndex = 1;
                        foreach (var weekPlanSet in weekPlan.WeekPlanSets)
                        {
                            carouselItem.Set = setIndex++;
                            carouselItem.Reps = weekPlanSet.Value;
                        }
                    }
                }
                if (workout.Exercise != null)
                {
                    carouselItem.Title = workout.Exercise.Name;
                    carouselItem.Subtitle = string.Join(", ", workout.Exercise.Muscles?.Select(m => m.Name) ?? new List<string> { "" });
                    carouselItem.Icon = "dotnet_bot.png";
                }
                serverData.Add(carouselItem);
            }
        }

        Items = new(serverData);
        IsFuckingBusy = false;
    }

    [RelayCommand]
    async Task RefreshAsync()
        => await OnAppearingAsync();
}
