using System.ComponentModel.DataAnnotations;

namespace Library.User.Create;

public class Payload
{
    public string Name { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }
}
