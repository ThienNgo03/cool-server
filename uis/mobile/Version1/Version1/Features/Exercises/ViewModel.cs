using Mvvm;
using Navigation;

namespace Version1.Features.Exercises;

public partial class ViewModel(IAppNavigator appNavigator,
                            Library.Exercises.Interface exercises ) : BaseViewModel(appNavigator)
{
	#region [ Fields ]

	private readonly Library.Exercises.Interface exercises = exercises;
    #endregion

    #region [ UI ]

    [ObservableProperty]
    ObservableCollection<ContentViews.Card.Model> items = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        var response = await exercises.AllAsync(new()
        {
            PageIndex = 0,
            PageSize = 20
        });

        Items.Add(new ContentViews.Card.Model()
        {
            Title = response.Title,
            Description = response.Detail,
            SubTitle = response.Data?.Items?.Count.ToString() ?? "0",
            IconUrl = "dotnet_bot.png"
        });

        foreach (var item in response.Data?.Items)
        {
            Items.Add(new ContentViews.Card.Model()
            {
                Title = item.Name,
                Description = item.Description,
                //SubTitle = item.MusclesWorked?.Count.ToString() ?? "0",
                IconUrl = "dotnet_bot.png"
            });
        }
    }
    #endregion
}
