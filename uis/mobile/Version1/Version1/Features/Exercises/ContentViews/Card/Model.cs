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
    ObservableCollection<Set> sets;

    [ObservableProperty]
    ObservableCollection<WeekPlan> weekPlans;
}

public class Set
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Value { get; set; }
}

public partial class WeekPlan
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public string DateOfWeek { get; set; } = string.Empty;

    public DateTime Time { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}