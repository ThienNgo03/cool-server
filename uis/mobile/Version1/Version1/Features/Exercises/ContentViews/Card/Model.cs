using Mvvm;

namespace Version1.Features.Exercises.ContentViews.Card;

public partial class Model : BaseModel
{
    [ObservableProperty]
    string id;

    [ObservableProperty]
    string title;

    [ObservableProperty]
    string subTitle;

    [ObservableProperty]
    string description;

    [ObservableProperty]
    string iconUrl;

    [ObservableProperty]
    bool isSelected;

    [ObservableProperty]
    bool isMondaySelected;

    [ObservableProperty]
    bool isTuesdaySelected;

    [ObservableProperty]
    bool isWednesdaySelected;

    [ObservableProperty]
    bool isThursdaySelected;

    [ObservableProperty]
    bool isFridaySelected;

    [ObservableProperty]
    bool isSaturdaySelected;

    [ObservableProperty]
    bool isSundaySelected;

    [ObservableProperty]
    ObservableCollection<Set> sets;

    [ObservableProperty]
    ObservableCollection<WeekPlan> weekPlans;
}

public partial class Set : BaseModel
{
    [ObservableProperty]
    Guid id;

    [ObservableProperty]
    string text;

    [ObservableProperty]
    int value;
}

public partial class WeekPlan : BaseModel
{
    [ObservableProperty]
    Guid id;

    [ObservableProperty]
    Guid workoutId;

    [ObservableProperty]
    string dateOfWeek;

    [ObservableProperty]
    TimeSpan time;

    [ObservableProperty]
    DateTime createdDate;

    [ObservableProperty]
    DateTime? lastUpdated;
}