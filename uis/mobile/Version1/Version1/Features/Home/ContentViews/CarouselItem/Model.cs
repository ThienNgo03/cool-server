using Mvvm;

namespace Version1.Features.Home.ContentViews.CarouselItem;

public partial class Model : BaseModel
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
