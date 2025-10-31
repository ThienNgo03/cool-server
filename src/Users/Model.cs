namespace Journal.Users;

public class Model
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string? ProfilePicture { get; set; }
}
