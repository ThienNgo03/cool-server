
using UI;
using Mvvm;
using Version1.UI.Attributes;

namespace Version1.Features.Authentication.SignIn;

public partial class Form : BaseFormModel
{

    [ObservableProperty]
    [Required(ErrorMessage = "Please enter your phone number or email address")]
    [PhoneOrEmail(ErrorMessage = "Please enter a valid phone number or email address")]
    [NotifyPropertyChangedFor(nameof(AccountErrors))]
    [NotifyDataErrorInfo]
    string account = "issacclark115@gmail.com";

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
    [NotifyPropertyChangedFor(nameof(PasswordErrors))]
    [NotifyDataErrorInfo]
    string password = "NewPassword@1";

    public IEnumerable<ValidationResult> AccountErrors => GetErrors(nameof(Account));
    public IEnumerable<ValidationResult> PasswordErrors => GetErrors(nameof(Password));

    protected override string[] ValidatableAndSupportPropertyNames => new[]
    {
        nameof(Account),
        nameof(AccountErrors),
        nameof(Password),
        nameof(PasswordErrors),
    };
}
