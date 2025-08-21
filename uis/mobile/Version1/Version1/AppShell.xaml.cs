namespace Version1;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();


        Routing.RegisterRoute("signUp", typeof(Features.Authentication.SignUp.Page));
        Routing.RegisterRoute("forgotPassword", typeof(Features.Authentication.ForgotPassword.Page));
    }
}
