using Mvvm;
using Navigation;

namespace Version1.Features.Exercises;

public partial class ViewModel(
    IAppNavigator appNavigator,
    Library.Muscles.Interface muscles,
    Library.Workouts.Interface workouts,
    Library.WeekPlanSets.Interface weekplanSet,
    Library.Exercises.Interface exercises) : BaseViewModel(appNavigator)
{
    #region [ Fields ]

    private readonly Library.Muscles.Interface muscles = muscles;
    private readonly Library.Exercises.Interface exercises = exercises;
    private readonly Library.Workouts.Interface workouts = workouts;
    private readonly Library.WeekPlanSets.Interface weekplanSet = weekplanSet;
    #endregion

    #region [ UI ]

    private ContentViews.Card.Model[] source = Array.Empty<ContentViews.Card.Model>();

    [ObservableProperty]
    bool isLoading;

    [ObservableProperty]
    ObservableCollection<string> tags = new();


    [ObservableProperty]
    ObservableCollection<ContentViews.Card.Model> items;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;

            var response = await exercises.AllAsync();
            Items = new();

            if (response?.Data?.Items == null)
            {
                return;
            }

            var serverData = new List<ContentViews.Card.Model>();

            foreach (var ex in response.Data.Items)
            {
                var card = new ContentViews.Card.Model
                {
                    Id = ex.Id.ToString(),
                    Title = ex.Name,
                    SubTitle = ex.Muscles != null && ex.Muscles.Any()
                        ? string.Join(", ", ex.Muscles.Select(m => m.Name))
                        : string.Empty,
                    Description = ex.Description,
                    IconUrl = "dotnet_bot.png",
                    Badge = "Easy",
                    BadgeTextColor = "#2b6cb0",
                    BadgeBackgroundColor = "#ebf8ff",
                    Progress = 50,
                };
                serverData.Add(card);
            }
            
            source = serverData.ToArray();
            Items = new ObservableCollection<ContentViews.Card.Model>(source);

            var tagResponse = await muscles.AllAsync();
            if (tagResponse?.Data?.Items == null)
                return;
            Tags = new ObservableCollection<string>(
                    tagResponse.Data.Items
                        .Select(m => m.Name)
            );
        }
        catch (Exception ex)
        {
            await AppNavigator.ShowSnackbarAsync($"Failed to load exercises");
            System.Diagnostics.Debug.WriteLine($"LoadAsync Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task NavigateAsync(string route, object args)
        => await AppNavigator.NavigateAsync(route, args: args, animated: true);

    #endregion

    #region [ Search ]

    [ObservableProperty]
    string searchTerm = string.Empty;

    partial void OnSearchTermChanged(string value)
    {
        IsClearButtonVisible = !string.IsNullOrWhiteSpace(value);
        ApplyFilters();
    }

    [ObservableProperty]
    bool isClearButtonVisible;

    [RelayCommand]
    public void ClearSearch() => SearchTerm = string.Empty;

    private CancellationTokenSource? _searchCancellationTokenSource;

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

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    filtered = filtered.Where(MatchesSearchTerm(SearchTerm));
                }

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
