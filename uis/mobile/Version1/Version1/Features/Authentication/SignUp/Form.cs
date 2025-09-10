using Mvvm;

namespace Version1.Features.Authentication.SignUp;

public partial class Form : BaseFormModel
{
    [ObservableProperty]
    string firstName;

    [ObservableProperty]
    string lastName;

    [ObservableProperty]
    string phoneNumber;

    [ObservableProperty]
    string email;

    [ObservableProperty]
    [Required(ErrorMessage = "Please enter a password")]
    string password;

    [ObservableProperty]
    string confirmPassword;

    [ObservableProperty]
    DateTime dateOfBirth = DateTime.Now;

    [ObservableProperty]
    double height;

    [ObservableProperty]
    double weight;

    [ObservableProperty]
    bool gender;

}
