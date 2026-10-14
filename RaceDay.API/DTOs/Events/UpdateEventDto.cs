using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Events;

public class UpdateEventDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? EventDate { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public decimal? Distance { get; set; }

    [RegularExpression("^(run|walk|cycle)$")]
    public string? EventType { get; set; }

    [MaxLength(1000)]
    public string? RouteInfo { get; set; }

    [MaxLength(500)]
    public string? WeatherInfo { get; set; }

    public bool? IsActive { get; set; }
}