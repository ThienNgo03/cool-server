using Mvvm;
using Navigation;

namespace Version1.Features.Authentication.SignUp;

public partial class ViewModel(IAppNavigator appNavigator) : BaseViewModel(appNavigator)
{

    [ObservableProperty]
    ImageSource avatarSource;

    [RelayCommand]
    public async Task<FileResult> PickAndShow(PickOptions options)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    // Set the AvatarSource directly from the file path
                    AvatarSource = ImageSource.FromFile(result.FullPath);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            // Handle exceptions appropriately
        }

        return null;
    }

}
