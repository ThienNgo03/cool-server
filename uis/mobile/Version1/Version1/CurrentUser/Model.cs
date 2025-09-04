
using Mvvm;

namespace Version1.CurrentUser;

public partial class Model : BaseModel
{
    [ObservableProperty]
    Guid id;

    [ObservableProperty]
    string name;

    [ObservableProperty]
    int? age;

    [ObservableProperty]
    string? avatarUrl;

    [ObservableProperty]
    string? bio;

    [ObservableProperty]
    string? token;
}
