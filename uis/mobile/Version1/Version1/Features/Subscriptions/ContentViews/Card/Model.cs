using Mvvm;

namespace Version1.Features.Subscriptions.ContentViews.Card;

public partial class Model : BaseModel
{
    [ObservableProperty]
    string id;
    [ObservableProperty]
    string title;
    [ObservableProperty]
    string subTitle;
    [ObservableProperty]
    decimal price;
    [ObservableProperty]
    string iconUrl;
    [ObservableProperty]
    string deadline;
    [ObservableProperty]
    string deadlineTextColor;
    [ObservableProperty]
    string deadlineBackgroundColor;
    [ObservableProperty]
    Status status;
}

public partial class Status : BaseModel
{
    [ObservableProperty]
    string? icon;
    [ObservableProperty]
    string title;
    [ObservableProperty]
    string titleTextColor;
    [ObservableProperty]
    string subtitle;
    [ObservableProperty]
    string subtitleTextColor;
    [ObservableProperty]
    string backgroundColor;
}
