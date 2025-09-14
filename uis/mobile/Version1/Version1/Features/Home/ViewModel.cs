using Mvvm;
using Navigation;

namespace Version1.Features.Home;

public partial class ViewModel(IAppNavigator appNavigator) : BaseViewModel(appNavigator)
{
    [ObservableProperty]
    List<CarouselItem> items = new();

    public override async Task OnAppearingAsync()
    {
        await base.OnAppearingAsync();
        IsFuckingBusy = true;
        await Task.Delay(2000); 
        Items = new List<CarouselItem>
        {
            new CarouselItem
            {
                Id = Guid.NewGuid(),
                Title = "Pull Up",
                Subtitle = "Legs and shoulders",
                Time = "17:00 PM",
                Set = 3,
                Reps = 10,
                Icon = "pullup_64.png"
            },
            new CarouselItem
            {
                Id = Guid.NewGuid(),
                Title = "Push Up",
                Subtitle = "Chests, frontdell and triceps",
                Time = "17:00 PM",
                Set = 4,
                Reps = 12,
                Icon = "pushup_64.png"
            },
            new CarouselItem
            {
                Id = Guid.NewGuid(),
                Title = "Squat",
                Subtitle = "Glutes and Quadriceps",
                Time = "17:00 PM",
                Set = 5,
                Reps = 8,
                Icon = "squat_64.png"
            }
        };
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