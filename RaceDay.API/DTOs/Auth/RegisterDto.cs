using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Auth;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, RegularExpression("^(Organiser|Participant)$",
        ErrorMessage = "Role must be Organiser or Participant")]
    public string Role { get; set; } = string.Empty;
}
