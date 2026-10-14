using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Enrolments;

public class EnrolDto
{
    [Required]
    public int CategoryId { get; set; }
}