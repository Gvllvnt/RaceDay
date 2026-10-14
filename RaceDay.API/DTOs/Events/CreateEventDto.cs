using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Events;

public class CreateEventDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal? Distance { get; set; }

    [RegularExpression("^(run|walk|cycle)$",
        ErrorMessage = "EventType must be run, walk, or cycle")]
    public string? EventType { get; set; }

    [MaxLength(1000)]
    public string? RouteInfo { get; set; }

    [MaxLength(500)]
    public string? WeatherInfo { get; set; }
}