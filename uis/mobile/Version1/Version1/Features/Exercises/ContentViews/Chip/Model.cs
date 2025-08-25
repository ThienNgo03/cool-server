
using Mvvm;

namespace Version1.Features.Exercises.ContentViews.Chip;

public partial class Model : BaseModel
{
    [ObservableProperty]
    string id;
    [ObservableProperty]
    string text;
    [ObservableProperty]
    bool isSelected;
}
