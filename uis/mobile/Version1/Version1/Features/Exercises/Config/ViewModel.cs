using Mvvm;
using Navigation;

namespace Version1.Features.Exercises.Config;

public partial class ViewModel(
    IAppNavigator appNavigator) : NavigationAwareBaseViewModel(appNavigator)
{
    #region [ UI ]

    [ObservableProperty]
    string id;

    protected override void OnInit(IDictionary<string, object> query)
    {
        base.OnInit(query);


        if (query.GetData<object>() is IDictionary<string, object> data &&
            data.TryGetValue("Id", out var idObj) &&
            idObj is string idStr)
        {
            Id = idStr;
        }

        TotalSets.CollectionChanged += (s, e) =>
        {
            SummaryTotalReps = TotalSets.Sum(x => x.Reps);
        };
    }
    #endregion

    #region [ Weekly Schedule ]

    [ObservableProperty]
    ObservableCollection<WeeklyItem> weeklyItems = new ObservableCollection<WeeklyItem>(
    Enumerable.Range(0, 7)
        .Select(i => new WeeklyItem
        {
            Id = Guid.NewGuid().ToString(),
            Content = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(i + 1) % 7],
            IsSelected = false
        }));
    #endregion

    #region [ Workout Times ]

    [ObservableProperty]
    ObservableCollection<WorkoutTimeItem>? workoutTimeItems = new();

    #endregion

    #region [ Sets For Days ]

    [ObservableProperty]
    string setConfigItemHeader = "Set For Days";

    [ObservableProperty]
    WeeklyItem? selectedDayForSet;

    partial void OnSelectedDayForSetChanged(WeeklyItem? value)
    {
        if (SetConfigItems == null) return;
        SetConfigItems.Clear();

        if (value != null)
        {
            var filteredItems = TotalSets
                .Where(item => string.Equals(item.Day, value.Content, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var item in filteredItems)
            {
                SetConfigItems.Add(item);
            }
        }
    }

    [ObservableProperty]
    ObservableCollection<WeeklyItem> selectedDays = new();

    public ObservableCollection<SetConfigItem> TotalSets { get; set; } = new();

    [ObservableProperty]
    ObservableCollection<SetConfigItem>? setConfigItems = new();

    #endregion

    #region [ Summary ]

    [ObservableProperty]
    int summaryTotalReps;
    #endregion
}

public partial class WeeklyItem : BaseModel
{
    [ObservableProperty]
    string id;

    [ObservableProperty]
    string content;

    [ObservableProperty]
    bool isSelected;
}

public partial class WorkoutTimeItem : BaseModel
{
    [ObservableProperty]
    string id;
    [ObservableProperty]
    string content;
    [ObservableProperty]
    TimeSpan time;
}

public partial class SetConfigItem : BaseModel
{
    [ObservableProperty]
    string id;
    [ObservableProperty] 
    string content;
    [ObservableProperty]
    string day;
    [ObservableProperty]
    int reps;
}
