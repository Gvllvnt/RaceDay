using System.Text.Json.Serialization;

namespace RaceDay.Web.Models;

// ---------- Responses coming back from the RaceDay API ----------

public class AuthResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class EventSummary
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? Distance { get; set; }
    public string? EventType { get; set; }
    public string? RouteInfo { get; set; }
    public string? BannerUrl { get; set; }
    public string? Organiser { get; set; }
    public int CategoryCount { get; set; }
    public int TotalEnrolments { get; set; }
}

public class OrganiserInfo
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CategoryInfo
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public TimeOnly StartTime { get; set; }
    public decimal EntryFee { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentEnrolments { get; set; }

    public int SpotsLeft => Math.Max(0, MaxParticipants - CurrentEnrolments);
    public bool IsFull => CurrentEnrolments >= MaxParticipants;
}

public class EventDetail
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? Distance { get; set; }
    public string? EventType { get; set; }
    public string? RouteInfo { get; set; }
    public string? WeatherInfo { get; set; }
    public string? BannerUrl { get; set; }
    public OrganiserInfo? Organiser { get; set; }
    public List<CategoryInfo> Categories { get; set; } = new();
}

public class MyEnrolment
{
    public int EnrolmentId { get; set; }
    public int? RaceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public int CategoryId { get; set; }
    public int EventId { get; set; }

    [JsonPropertyName("event")]
    public string EventTitle { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public DateTime EventDate { get; set; }
}

public class EventEnrolment
{
    public int EnrolmentId { get; set; }
    public int? RaceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public string Participant { get; set; } = string.Empty;
    public string ParticipantEmail { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public int? ResultId { get; set; }
    public TimeOnly? FinishTime { get; set; }
    public int? Position { get; set; }
    public string? Pace { get; set; }
}

public class MyResult
{
    public int ResultId { get; set; }
    public TimeOnly? FinishTime { get; set; }
    public int? Position { get; set; }
    public string? Pace { get; set; }

    [JsonPropertyName("event")]
    public string EventTitle { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public int TotalFinishers { get; set; }
}

public class ProfileInfo
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EnrolmentCreated
{
    public int EnrolmentId { get; set; }
    public int? RaceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ImageUploaded
{
    public string? BannerUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

// ---------- Requests sent to the RaceDay API ----------

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class EventRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? Distance { get; set; }
    public string? EventType { get; set; }
    public string? RouteInfo { get; set; }
    public string? WeatherInfo { get; set; }
}

public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public TimeOnly StartTime { get; set; }
    public decimal EntryFee { get; set; }
    public int MaxParticipants { get; set; }
}

public class ResultRequest
{
    public TimeOnly? FinishTime { get; set; }
    public int? Position { get; set; }
}

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
}
