using Mvvm;
using Navigation;
using System.Collections.Specialized;

namespace Version1.Features.Exercises.Config;

public partial class ViewModel(
    Library.Workouts.Interface workoutsBiz,
    IAppNavigator appNavigator) : NavigationAwareBaseViewModel(appNavigator)
{
    #region [ Fields ]

    private readonly Library.Workouts.Interface workoutsBiz = workoutsBiz;
    #endregion

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
        TotalSets.CollectionChanged += TotalSets_CollectionChanged;
    }

    [RelayCommand]
    async Task SaveAsync()
    {
        if (Id is null)
        {
            await ShowSnackBarAsync("Exercise Id is missing.");
            return;
        }
        if (WeeklyItems is null || !WeeklyItems.Any())
        {
            await ShowSnackBarAsync("Please select at least one day for the workout.");
            return;
        }
        if (WorkoutTimeItems is null || !WorkoutTimeItems.Any())
        {
            await ShowSnackBarAsync("Workout times are not configured.");
            return;
        }
        if (SetConfigItems is null || !SetConfigItems.Any())
        {
            await ShowSnackBarAsync("Set configurations are missing.");
            return;
        }

        await workoutsBiz.CreateAsync(new Library.Workouts.Create.Payload
        {
            ExerciseId = Guid.Parse(Id),
            UserId = Guid.Parse("fdfa4136-ada3-41dc-b16e-8fd9556d4574"), // đang cứng data
            WeekPlans = WorkoutTimeItems?.Select(wti => new Library.Workouts.Create.WeekPlan
            {
                DateOfWeek = wti.Content,
                Time = wti.Time,
                WeekPlanSets = SetConfigItems?.Select(sci => new Library.Workouts.Create.WeekPlanSet
                {
                    Value = sci.Reps
                }).ToList()
            }).ToList()
        });
    }

    public async Task ShowSnackBarAsync(string message)
    {
        await AppNavigator.ShowSnackbarAsync(message);
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
        UpdateSetConfigItems();
    }

    [ObservableProperty]
    ObservableCollection<WeeklyItem> selectedDays = new();

    public ObservableCollection<SetConfigItem> TotalSets { get; set; } = new();

    [ObservableProperty]
    ObservableCollection<SetConfigItem>? setConfigItems = new();

    private void TotalSets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SummaryTotalReps = TotalSets.Sum(x => x.Reps);
        UpdateSetConfigItems();
    }

    private void UpdateSetConfigItems()
    {
        if (SetConfigItems == null) return;
        SetConfigItems.Clear();

        if (SelectedDayForSet != null)
        {
            var filteredItems = TotalSets
                .Where(item => string.Equals(item.Day, SelectedDayForSet.Content, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var item in filteredItems)
            {
                SetConfigItems.Add(item);
            }
        }
    }
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
