using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Enrolment
{
    [Key]
    public int EnrolmentId { get; set; }

    [Required]
    public int ParticipantId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int? RaceNumber { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Registered";

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ParticipantId))]
    public User? Participant { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public Result? Result { get; set; }
}
