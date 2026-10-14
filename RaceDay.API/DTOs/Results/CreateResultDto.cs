using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Results;

public class CreateResultDto
{
    public TimeOnly? FinishTime { get; set; }

    public int? Position { get; set; }

    [MaxLength(20)]
    public string? Pace { get; set; }
}