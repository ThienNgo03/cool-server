using System.ComponentModel.DataAnnotations;

namespace Journal.Identity.Signin;

public class Payload
{
    [Required]
    public string AccountEmail { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
