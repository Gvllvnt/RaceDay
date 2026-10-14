using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Categories;

public class CreateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, Range(0.1, 500)]
    public decimal DistanceKm { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required, Range(0, 100000)]
    public decimal EntryFee { get; set; }

    [Required, Range(1, 100000)]
    public int MaxParticipants { get; set; }
}
