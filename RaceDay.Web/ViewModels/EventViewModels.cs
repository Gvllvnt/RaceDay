using System.ComponentModel.DataAnnotations;
using RaceDay.Web.Models;

namespace RaceDay.Web.ViewModels;

public class HomeIndexViewModel
{
    public string? Search { get; set; }
    public string? EventType { get; set; }
    public string? Location { get; set; }
    public List<EventSummary> Events { get; set; } = new();
    public string? LoadError { get; set; }
    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrWhiteSpace(EventType) || !string.IsNullOrWhiteSpace(Location);
}

public class EventDetailsViewModel
{
    public EventDetail Event { get; set; } = new();
    public HashSet<int> EnrolledCategoryIds { get; set; } = new();
    public bool HasPassed => Event.EventDate.Date < DateTime.Today;
}

public class EventFormViewModel
{
    public int EventId { get; set; }

    [Required(ErrorMessage = "Please enter the event name.")]
    [MaxLength(200, ErrorMessage = "The name can be at most 200 characters.")]
    [Display(Name = "Event name")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "The description can be at most 1000 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please choose the event date and start time.")]
    [Display(Name = "Date and start time")]
    public DateTime? EventDate { get; set; }

    [Required(ErrorMessage = "Please enter the event location.")]
    [MaxLength(200, ErrorMessage = "The location can be at most 200 characters.")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the main distance in kilometres.")]
    [Range(0.1, 1000, ErrorMessage = "The distance must be between 0.1 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal? Distance { get; set; }

    [Required(ErrorMessage = "Please choose an event type.")]
    [RegularExpression("^(run|walk|cycle)$", ErrorMessage = "The event type must be Run, Walk or Cycle.")]
    [Display(Name = "Event type")]
    public string? EventType { get; set; }

    [MaxLength(1000, ErrorMessage = "Route information can be at most 1000 characters.")]
    [Display(Name = "Route information")]
    public string? RouteInfo { get; set; }

    [MaxLength(500, ErrorMessage = "Weather information can be at most 500 characters.")]
    [Display(Name = "Weather information")]
    public string? WeatherInfo { get; set; }

    [Display(Name = "Banner image")]
    public IFormFile? BannerFile { get; set; }

    public string? ExistingBannerUrl { get; set; }

    public bool IsEdit => EventId > 0;
}

public class DashboardViewModel
{
    public List<EventSummary> Events { get; set; } = new();
    public int TotalEvents => Events.Count;
    public int TotalEnrolments => Events.Sum(e => e.TotalEnrolments);
    public List<EventSummary> Upcoming => Events
        .Where(e => e.EventDate.Date >= DateTime.Today)
        .OrderBy(e => e.EventDate)
        .ToList();
    public List<EventSummary> Past => Events
        .Where(e => e.EventDate.Date < DateTime.Today)
        .OrderByDescending(e => e.EventDate)
        .ToList();
}
