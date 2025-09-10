namespace Version1.Features.Authentication.SignUp;

public partial class Page : ContentPage
{
    #region [ Fields ]

    private readonly ViewModel viewModel;
    #endregion

    #region [ CTors ]

    public Page(ViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = this.viewModel = viewModel;
    }
    #endregion

    private async void CameraButton_Clicked(object sender, EventArgs e)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
            return;

        FileResult avatar = await MediaPicker.Default.CapturePhotoAsync();
        if (avatar is null)
            return;

        viewModel.AvatarSource = ImageSource.FromFile(avatar.FullPath);
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
    }
}