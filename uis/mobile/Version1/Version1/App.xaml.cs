namespace Version1;

public partial class App : Application
{
    public CurrentUser.Model? CurrentUser { get; set; }

    public App()
    {
        InitializeComponent();
        Microsoft.Maui.Handlers.ToolbarHandler.Mapper.AppendToMapping("CustomNavigationView", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.ContentInsetStartWithNavigation = 0;
            handler.PlatformView.SetContentInsetsAbsolute(0, 0);
#endif
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    public void SetCurrentUser(Guid id, 
                               string name, 
                               int? age = null, 
                               string? avatarUrl = null, 
                               string? bio = null,
                               string? token = null)
    {
        CurrentUser = new CurrentUser.Model
        {
            Id = id,
            Name = name,
            Age = age,
            AvatarUrl = avatarUrl,
            Bio = bio,
            Token = token
        };
    }
}