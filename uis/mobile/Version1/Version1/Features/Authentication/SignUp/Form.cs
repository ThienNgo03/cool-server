using Mvvm;
using UI;

namespace Version1.Features.Authentication.SignUp;

public partial class Form : BaseFormModel
{
    [ObservableProperty]
    [Required(ErrorMessage = "Please enter your First name ")]
    string firstName = "Khoa";

    [ObservableProperty]
    [Required(ErrorMessage = "Please enter your Last name")]
    string lastName = "VoDuy";

    [ObservableProperty]
    [Required(ErrorMessage = "Please enter your Phone number")]
    [Phone(ErrorMessage = "Please enter a valid phone number or email address")]
    string phoneNumber = "0918761646";

    [ObservableProperty]
    [Required(ErrorMessage = "Please enter your Email")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    string email = "Khoavd2003@gmail.com";

    [ObservableProperty]
    [Required(ErrorMessage = "Please enter a password")]
    [Password(
        IncludesLower = true,
        IncludesNumber = true,
        IncludesSpecial = true,
        IncludesUpper = true,
        MinimumLength = 6,
        ErrorMessage = "Please enter a strong password: from 8 characters, 1 upper, 1 lower, 1 digit, 1 special character"
    )]
    string password = "Duykhoa@123";

    [ObservableProperty]
    [Required(ErrorMessage = "Please confirm password")]
    [FieldCompare(nameof(Password), ErrorMessage = "Password and Confirm Password do not match")]
    string confirmPassword = "Duykhoa@123";

    [ObservableProperty]
    [Required(ErrorMessage = "Please enter your DOB")]
    DateTime dateOfBirth = DateTime.Now;

    [ObservableProperty]
    [Required(ErrorMessage = "Please enter your height")]
    double height = 170;

    [ObservableProperty]
    [Required(ErrorMessage = "Please enter your weight")]
    double weight = 75;

    [ObservableProperty]
    bool gender = true;

}
