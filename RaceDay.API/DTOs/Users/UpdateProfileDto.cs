using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Users;

public class UpdateProfileDto
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
