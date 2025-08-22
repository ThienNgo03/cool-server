using Mvvm;

namespace Version1.Features.Exercises;

public partial class Model : BaseModel
{
    [ObservableProperty]
    Guid id;

    [ObservableProperty]
    string name;

    [ObservableProperty]
    string description;

    [ObservableProperty]
    string type;

    [ObservableProperty]
    DateTime createdDate;

    [ObservableProperty]
    DateTime lastUpdated;
}
