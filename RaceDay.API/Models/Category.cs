using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    public int EventId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal DistanceKm { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public decimal EntryFee { get; set; }

    [Required]
    public int MaxParticipants { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(EventId))]
    public Event? Event { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}