namespace Journal.Profiles;

public class Model: Models.Base
{
    public string Name { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string? ProfilePicture { get; set; }
}
