using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Result
{
    [Key]
    public int ResultId { get; set; }

    [Required]
    public int EnrolmentId { get; set; }

    public TimeOnly? FinishTime { get; set; }

    public int? Position { get; set; }

    [MaxLength(20)]
    public string? Pace { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(EnrolmentId))]
    public Enrolment? Enrolment { get; set; }
}
