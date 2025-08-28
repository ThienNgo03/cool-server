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
    #endregion

    #region [ UI ]

    [ObservableProperty]
    ObservableCollection<ContentViews.Card.Model> items = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        var exercises = await LoadExercisesAsync();
        var testUser = Guid.Parse("84c361c8-eb0b-415e-a525-8c04992dec47");
        var workouts = await LoadWorkoutsAsync(userId: testUser,
                                               isIncludeWeekPlans: true,
                                               isIncludeWeekPlanSets: true);

        foreach (var exercise in exercises)
        {
            // Find matching workout for this exercise
            var matchingWorkout = workouts.FirstOrDefault(w => w.ExerciseId == exercise.Id);

            var cardModel = new ContentViews.Card.Model()
            {
                Id = exercise.Id.ToString(),
                Title = exercise.Name,
                Description = exercise.Description,
                IconUrl = "dotnet_bot.png",
                IsSelected = matchingWorkout != null, 
                Sets = new ObservableCollection<ContentViews.Card.Set>(),
                WeekPlans = new ObservableCollection<ContentViews.Card.WeekPlan>()
            };

            if (matchingWorkout != null)
            {
                // Map week plans
                if (matchingWorkout.WeekPlans != null)
                {
                    foreach (var weekPlan in matchingWorkout.WeekPlans)
                    {
                        cardModel.WeekPlans.Add(new ContentViews.Card.WeekPlan
                        {
                            Id = weekPlan.Id,
                            WorkoutId = weekPlan.WorkoutId,
                            DateOfWeek = weekPlan.DateOfWeek,
                            Time = weekPlan.Time,
                            CreatedDate = weekPlan.CreatedDate,
                            LastUpdated = weekPlan.LastUpdated
                        });

                        switch (weekPlan.DateOfWeek?.ToLower())
                        {
                            case "monday":
                                cardModel.IsMondaySelected = true;
                                break;
                            case "tuesday":
                                cardModel.IsTuesdaySelected = true;
                                break;
                            case "wednesday":
                                cardModel.IsWednesdaySelected = true;
                                break;
                            case "thursday":
                                cardModel.IsThursdaySelected = true;
                                break;
                            case "friday":
                                cardModel.IsFridaySelected = true;
                                break;
                            case "saturday":
                                cardModel.IsSaturdaySelected = true;
                                break;
                            case "sunday":
                                cardModel.IsSundaySelected = true;
                                break;
                        }

                        if (weekPlan.WeekPlanSets != null)
                        {
                            foreach (var weekPlanSet in weekPlan.WeekPlanSets)
                            {
                                cardModel.Sets.Add(new ContentViews.Card.Set
                                {
                                    Id = weekPlanSet.Id,
                                    Text = $"Set {cardModel.Sets.Count + 1}",
                                    Value = weekPlanSet.Value
                                });
                            }
                        }
                    }
                }
            }

            Items.Add(cardModel);
        }
    }

    #endregion

    #region [ Utils ]

    async Task<ICollection<Library.Exercises.Model>> LoadExercisesAsync()
    {
        var response = await this.exercises.AllAsync();

        if(response == null || response.Data == null || response.Data.Items == null || response.Data.Items.Count == 0)
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
        if(item == null) return;

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
