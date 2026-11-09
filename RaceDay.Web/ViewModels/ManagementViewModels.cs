using System.ComponentModel.DataAnnotations;
using RaceDay.Web.Models;

namespace RaceDay.Web.ViewModels;

public class CategoryFormViewModel
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }

    [Required(ErrorMessage = "Please enter a category name.")]
    [MaxLength(100, ErrorMessage = "The name can be at most 100 characters.")]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the distance in kilometres.")]
    [Range(0.1, 500, ErrorMessage = "The distance must be between 0.1 and 500 km.")]
    [Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }

    [Required(ErrorMessage = "Please choose a start time.")]
    [Display(Name = "Start time")]
    public TimeOnly? StartTime { get; set; }

    [Required(ErrorMessage = "Please enter the entry fee (use 0 for free).")]
    [Range(0, 100000, ErrorMessage = "The entry fee must be between R0 and R100 000.")]
    [Display(Name = "Entry fee (R)")]
    public decimal? EntryFee { get; set; }

    [Required(ErrorMessage = "Please enter the maximum number of participants.")]
    [Range(1, 100000, ErrorMessage = "Capacity must be between 1 and 100 000.")]
    [Display(Name = "Max participants")]
    public int? MaxParticipants { get; set; }
}

public class CategoryManagementViewModel
{
    public EventDetail Event { get; set; } = new();
    public CategoryFormViewModel NewCategory { get; set; } = new();
}

public class EnrolmentManagementViewModel
{
    public EventDetail Event { get; set; } = new();
    public List<EventEnrolment> Enrolments { get; set; } = new();
    public string? CategoryFilter { get; set; }
    public List<string> CategoryNames => Event.Categories.Select(c => c.Name).ToList();

    public static readonly string[] Statuses = { "Pending", "Registered", "Confirmed", "Completed", "DNS", "DNF" };
}

public class ResultManagementViewModel
{
    public EventDetail Event { get; set; } = new();
    public List<EventEnrolment> Enrolments { get; set; } = new();

    /// <summary>Results can only be captured once the event day has arrived.</summary>
    public bool HasConcluded => Event.EventDate.Date <= DateTime.Today;
}
