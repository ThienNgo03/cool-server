using Mvvm;
using Navigation;

namespace Version1.Features.Exercises;

public partial class ViewModel(
    IAppNavigator appNavigator,
    Library.Workouts.Interface workouts,
    Library.Exercises.Interface exercises,
    Library.WeekPlanSets.Interface weekPlanSets) : BaseViewModel(appNavigator)
{
    #region [ Fields ]

    private readonly Library.Workouts.Interface workouts = workouts;
    private readonly Library.Exercises.Interface exercises = exercises;
    private readonly Library.WeekPlanSets.Interface weekplanSet = weekPlanSets;
    private readonly ContentViews.Card.Model[] source = new ContentViews.Card.Model[]
    {
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Squat",
            SubTitle = "Legs",
            Description = "A squat is a strength exercise in which the trainee lowers their hips from a standing position and then stands back up. During the descent, the hip and knee joints flex while the ankle joint dorsiflexes; conversely the hip and knee joints extend and the ankle joint plantarflexes when standing up.",
            IconUrl = "squat_64.png",
            Badge = "Easy",
            BadgeTextColor = "#2f8557",
            BadgeBackgroundColor = "#dbfce7",
            Progress = 45
        },
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Lunges",
            SubTitle = "Legs & Glutes",
            Description = "Lunges strengthen the lower body by targeting the quads, hamstrings, and glutes. They also improve balance and coordination.",
            IconUrl = "lunges_64.png",
            Badge = "Easy",
            BadgeTextColor = "#2f8557",
            BadgeBackgroundColor = "#dbfce7",
            Progress = 90
        },
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Triceps Extension",
            SubTitle = "Arms",
            Description = "Triceps extensions isolate and strengthen the triceps muscles, helping to tone the back of the arms and improve upper body strength.",
            IconUrl = "triceps_64.png",
            Badge = "Medium",
            BadgeTextColor = "#2b6cb0",
            BadgeBackgroundColor = "#ebf8ff",
        },
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Pull-Up",
            SubTitle = "Back & Biceps",
            Description = "Pull-ups are a compound upper-body exercise that build strength in the lats, biceps, and shoulders by lifting your body over a bar.",
            IconUrl = "pullup_64.png",
            Badge = "Hard",
            BadgeTextColor = "#c53030",
            BadgeBackgroundColor = "#fed7d7",
        },
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Push-Up",
            SubTitle = "Chest & Triceps",
            Description = "Push-ups are a classic bodyweight exercise that target the chest, shoulders, and triceps while also engaging the core.",
            IconUrl = "pushup_64.png",
            Badge = "Easy",
            BadgeTextColor = "#2b6cb0",
            BadgeBackgroundColor = "#ebf8ff",
        },
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Plank",
            SubTitle = "Core",
            Description = "The plank is an isometric hold that strengthens the core, shoulders, and glutes while improving posture and stability.",
            IconUrl = "plank_64.png",
            Badge = "Medium",
            BadgeTextColor = "#b7791f",
            BadgeBackgroundColor = "#fefcbf",
        },
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Pilates",
            SubTitle = "Core & Flexibility",
            Description = "Pilates is a low-impact exercise that focuses on core strength, flexibility, and mindful movement. It improves posture and body awareness.",
            IconUrl = "pilates_64.png",
            Badge = "Medium",
            BadgeTextColor = "#b7791f",
            BadgeBackgroundColor = "#fefcbf",
        },
    };
    #endregion

    #region [ UI ]

    [ObservableProperty]
    bool isLoading;

    [ObservableProperty]
    bool isClearButtonVisible;

    [RelayCommand]
    public void ClearSearch() => SearchTerm = string.Empty;

    [ObservableProperty]
    ObservableCollection<string> tags = new()
    {
        "Chest",
        "Back",
        "Legs",
        "Arms",
        "Core",
        "Biceps",
        "Triceps",
        "Glutes",
        "Flexibility"
    };

    [ObservableProperty]
    ObservableCollection<ContentViews.Card.Model> items;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        Items = new(source);
        IsLoading = false;
    }

    public async Task NavigateAsync(string route, object args)
        => await AppNavigator.NavigateAsync(route, args: args, animated: true);

    #endregion

    #region [ Search ]

    [ObservableProperty]
    string searchTerm = string.Empty;

    private CancellationTokenSource? _searchCancellationTokenSource;

    partial void OnSearchTermChanged(string value)
    {
        IsClearButtonVisible = !string.IsNullOrWhiteSpace(value);
        ApplyFilters();
    }


    private static Func<ContentViews.Card.Model, bool> MatchesSearchTerm(string searchTerm) =>
        item => new[] { item.Title, item.SubTitle, item.Description }
            .Where(field => !string.IsNullOrEmpty(field))
            .Any(field => field.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

    #endregion

    #region [ Filtering ]

    [ObservableProperty]
    private ObservableCollection<string> selectedTags = new();

    private CancellationTokenSource? _filterCancellationTokenSource;

    public void AddSelectedTag(string tag)
    {
        if (!SelectedTags.Contains(tag))
        {
            SelectedTags.Add(tag);
            ApplyFilters();
        }
    }

    public void RemoveSelectedTag(string tag)
    {
        if (SelectedTags.Contains(tag))
        {
            SelectedTags.Remove(tag);
            ApplyFilters();
        }
    }

    private void ApplyFilters()
    {
        _filterCancellationTokenSource?.Cancel();
        _filterCancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(100, _filterCancellationTokenSource.Token);

                var filtered = source.AsEnumerable();

                // Apply search filter if search term exists
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    filtered = filtered.Where(MatchesSearchTerm(SearchTerm));
                }

                // Apply tag filter if any tags are selected
                if (SelectedTags?.Count > 0)
                {
                    filtered = filtered.Where(MatchesSelectedTags);
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Items = new ObservableCollection<ContentViews.Card.Model>(filtered);
                });
            }
            catch (OperationCanceledException) { }
        });
    }

    private bool MatchesSelectedTags(ContentViews.Card.Model item)
    {
        if (SelectedTags == null || SelectedTags.Count == 0)
            return true;

        // Check if the item's SubTitle contains any of the selected tags
        return SelectedTags.Any(tag =>
            item.SubTitle?.Contains(tag, StringComparison.OrdinalIgnoreCase) == true);
    }

    #endregion


    #region [ Utils ]

    async Task<ICollection<Library.Exercises.Model>> LoadExercisesAsync()
    {
        var response = await this.exercises.AllAsync();

        if (response == null || response.Data == null || response.Data.Items == null || response.Data.Items.Count == 0)
            return new ObservableCollection<Library.Exercises.Model>();

        return response.Data.Items;
    }

    async Task<ICollection<Library.Workouts.Model>> LoadWorkoutsAsync(
        Guid? userId,
        bool isIncludeWeekPlans,
        bool isIncludeWeekPlanSets)
    {
        var response = await this.workouts.AllAsync(new()
        {
            UserId = userId,
            IsIncludeWeekPlans = isIncludeWeekPlans,
            IsIncludeWeekPlanSets = isIncludeWeekPlanSets
        });

        if (response == null || response.Data == null || response.Data.Items == null || response.Data.Items.Count == 0)
            return new ObservableCollection<Library.Workouts.Model>();

        return response.Data.Items;
    }

    async Task<ICollection<ContentViews.Chip.Model>> LoadMusclesAsync()
    {
        var response = await exercises.AllAsync();
        ObservableCollection<ContentViews.Chip.Model> items = new();
        foreach (var item in response.Data?.Items)
        {
            items.Add(new ContentViews.Chip.Model()
            {
                Id = item.Id.ToString(),
                Text = item.Name,
                IsSelected = false
            });
        }
        return items;
    }

    public async Task UpdateCardAsync(string id, bool isSelected)
    {
        var item = Items.FirstOrDefault(x => x.Id == id);
        if (item == null) return;

        item.IsMondaySelected = isSelected;
        item.IsWednesdaySelected = isSelected;
        item.IsFridaySelected = isSelected;
    }

    public async Task AddSetAsync(Guid weekPlanId, int value)
    {
        await weekplanSet.CreateAsync(new()
        {
            WeekPlanId = weekPlanId,
            Value = value
        });
    }

    public async Task AdjustSetAsync(Guid setId, int value)
    {
        await weekplanSet.PatchAsync(new()
        {
            Id = setId,
            Operations = new List<Library.Models.Patch.Operation>
            {
                new Library.Models.Patch.Operation
                {
                    Path = "Value",
                    Value = value
                }
            }
        });
    }

    public async Task AddDayOfWeekAsync()
    {

    }

    public async Task UpdateDayOfWeekAsync(string id, DayOfWeek dayOfWeek, bool isSelected)
    {
        var item = Items.FirstOrDefault(x => x.Id == id);
        if (item == null) return;
        switch (dayOfWeek)
        {
            case DayOfWeek.Monday:
                item.IsMondaySelected = isSelected;
                break;
            case DayOfWeek.Tuesday:
                item.IsTuesdaySelected = isSelected;
                break;
            case DayOfWeek.Wednesday:
                item.IsWednesdaySelected = isSelected;
                break;
            case DayOfWeek.Thursday:
                item.IsThursdaySelected = isSelected;
                break;
            case DayOfWeek.Friday:
                item.IsFridaySelected = isSelected;
                break;
            case DayOfWeek.Saturday:
                item.IsSaturdaySelected = isSelected;
                break;
            case DayOfWeek.Sunday:
                item.IsSundaySelected = isSelected;
                break;
        }
    }
    #endregion

    #region [ WebSocket ]


    [ObservableProperty]
    bool isAutoReconnect = true;

    [ObservableProperty]
    List<string> events = new()
    {
        "workout-created",
        "workout-updated",
        "workout-deleted"
    };

    [RelayCommand]
    async Task HandleSocketReportsPayloadAsync(WebSocket.SignalR.Payload payload)
    {
        if (payload is null || string.IsNullOrEmpty(payload.Event) || string.IsNullOrEmpty(payload.Id))
            return;

        switch (payload.Event)
        {
            case "workout-created":
                await HandleWorkoutCreatedAsync(payload.Id);
                break;
            case "workout-updated":
                await HandleReportUpdatedAsync(payload.Id);
                break;
            case "workout-deleted":
                await HandleReportDeletedAsync(payload.Id);
                break;
            default:
                break;
        }
    }

    private async Task HandleWorkoutCreatedAsync(string id)
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {

        });
    }

    private async Task HandleReportUpdatedAsync(string id)
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
        });
    }

    private async Task HandleReportDeletedAsync(string id)
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
        });
    }
    #endregion
}
