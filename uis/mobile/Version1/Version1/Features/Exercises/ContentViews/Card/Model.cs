using Mvvm;

namespace Version1.Features.Exercises.ContentViews.Card;

public partial class Model : BaseModel
{
    [ObservableProperty]
    string title;

    [ObservableProperty]
    string subTitle;

    [ObservableProperty]
    string description;

    [ObservableProperty]
    string iconUrl;
}
