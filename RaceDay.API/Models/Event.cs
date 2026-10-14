using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Event
{
    [Key]
    public int EventId { get; set; }

    [Required]
    public int OrganiserId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal? Distance { get; set; }

    [MaxLength(20)]
    public string? EventType { get; set; }  // run, walk, cycle

    [MaxLength(1000)]
    public string? RouteInfo { get; set; }

    [MaxLength(500)]
    public string? WeatherInfo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(OrganiserId))]
    public User? Organiser { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
}