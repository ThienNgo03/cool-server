using System.Text.RegularExpressions;

namespace Version1.UI.Attributes;

public class PhoneOrEmailAttribute : ValidationAttribute
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex = new Regex(
        @"^(\+\d{1,3}[- ]?)?\d{10}$|^(\+\d{1,3}[- ]?)?\d{3}[- ]?\d{3}[- ]?\d{4}$",
        RegexOptions.Compiled);

    public override bool IsValid(object value)
    {
        if (value is not string stringValue)
        {
            return base.IsValid(value);
        }

        if (string.IsNullOrWhiteSpace(stringValue))
            return false;

        stringValue = stringValue.Trim();

        // Check if it's a valid email
        if (EmailRegex.IsMatch(stringValue))
            return true;

        // Check if it's a valid phone number
        if (PhoneRegex.IsMatch(stringValue))
            return true;

        return false;
    }
}
