using Mvvm;
using Navigation;

namespace Version1.Features.Home;

public partial class ViewModel(IAppNavigator appNavigator, Library.Workouts.Interface workouts) : BaseViewModel(appNavigator)
{
    
    private readonly Library.Workouts.Interface workouts = workouts;

    [ObservableProperty]
    List<CarouselItem> items = new();

    public override async Task OnAppearingAsync()
    {
        await base.OnAppearingAsync();
        IsFuckingBusy = true;
        await Task.Delay(2000); 

        var response = await workouts.AllAsync( new Library.Workouts.All.Parameters {
            UserId = MyApp?.CurrentUser?.Id,
            IsIncludeWeekPlans = true,
            IsIncludeWeekPlanSets = true,
            IsIncludeExercises = true,
            IsIncludeMuscles = true,
        });

        if (response.Data?.Items == null)
        {
            IsFuckingBusy = false;
            return;
        }

        var serverData = new List<CarouselItem>();
        var dateOfWeek = DateTime.Today.DayOfWeek.ToString();
        foreach (var workout in response.Data.Items)
        {
            var weekPlans = workout.WeekPlans?.Where(wp => wp.DateOfWeek == dateOfWeek).ToList();
            if (weekPlans != null && weekPlans.Any())
            {
                var carouselItem = new CarouselItem { Id = workout.Id };
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

        Items = serverData;
        IsFuckingBusy = false;
    }
}

public partial class CarouselItem : BaseModel
{
    [ObservableProperty]
    Guid id;

    [ObservableProperty]
    string title;

    [ObservableProperty]
    string subtitle;

    [ObservableProperty]
    string time;

    [ObservableProperty]
    int set;

    [ObservableProperty]
    int reps;

    [ObservableProperty]
    string icon;
}