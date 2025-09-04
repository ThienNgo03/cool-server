namespace Version1;

public partial class App : Application
{
    public CurrentUser.Model? CurrentUser { get; set; }

    public App()
    {
        InitializeComponent();
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