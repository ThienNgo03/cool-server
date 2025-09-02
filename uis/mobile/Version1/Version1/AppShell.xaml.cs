namespace Version1;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();


        Routing.RegisterRoute("signUp", typeof(Features.Authentication.SignUp.Page));
        Routing.RegisterRoute("forgotPassword", typeof(Features.Authentication.ForgotPassword.Page));
        Routing.RegisterRoute("exercise-detail", typeof(Features.Exercises.Detail.Page));
        Routing.RegisterRoute("exercise-config", typeof(Features.Exercises.Config.Page));
    }
}
