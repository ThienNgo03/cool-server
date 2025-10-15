using Mvvm;
using Navigation;

namespace Version1.Features.Exercises;

public partial class ViewModel(
    IAppNavigator appNavigator,
    //Library.Muscles.Interface muscles,
    //Library.Exercises.Interface exercises,
    Core.Exercises.Interface exercises ) : BaseViewModel(appNavigator)
{
    #region [ Fields ]

    //private readonly Library.Muscles.Interface muscles = muscles;
    //private readonly Library.Exercises.Interface exercises = exercises;
    private readonly Core.Exercises.Interface exercises = exercises;
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

            var response = await exercises.AllAsync(new());
            Items = new();

            if (response == null)
            {
                return;
            }

            var serverData = new List<ContentViews.Card.Model>();

            foreach (var ex in response)
            {
                var card = new ContentViews.Card.Model
                {
                    Id = ex.Id.ToString(),
                    Title = ex.Title,
                    SubTitle = ex.SubTitle,
                    Description = ex.Description,
                    IconUrl = ex.ImageUrl,
                    Badge = ex.Badge,
                    BadgeTextColor = ex.BadgeTextColor,
                    BadgeBackgroundColor = ex.BadgeBackgroundColor,
                    Progress = ex.PercentageComplete,
                };
                serverData.Add(card);
            }
            
            source = serverData.ToArray();
            Items = new ObservableCollection<ContentViews.Card.Model>(source);

            var tagResponse = await exercises.CategoriesAsync();
            if (tagResponse == null)
                return;
            Tags = new ObservableCollection<string>(tagResponse);
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
