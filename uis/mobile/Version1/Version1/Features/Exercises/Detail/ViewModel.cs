using Mvvm;
using Navigation;

namespace Version1.Features.Exercises.Detail;

public partial class ViewModel(
    IAppNavigator appNavigator) : NavigationAwareBaseViewModel(appNavigator)
{
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
    }


}
