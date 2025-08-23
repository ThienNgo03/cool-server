using System.ComponentModel.DataAnnotations;

namespace Library.Authentication.Signin;
public class Payload
{
    [Required]
    public string Account { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
