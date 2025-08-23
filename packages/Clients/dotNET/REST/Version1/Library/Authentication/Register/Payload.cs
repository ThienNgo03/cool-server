using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Library.Authentication.Register;

public class Payload : IdentityUser
{
    [Required]
    public string AccountName { get; set; } = string.Empty;
    [Required]
    public string AccountEmail { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

}
