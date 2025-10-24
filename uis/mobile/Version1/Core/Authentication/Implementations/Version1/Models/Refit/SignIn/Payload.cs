
using System.ComponentModel.DataAnnotations;

namespace Core.Authentication.Implementations.Version1.Models.Refit.SignIn;
public class Payload
{
    [Required]
    public string Account { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
