using System.ComponentModel.DataAnnotations;
using RaceDay.Web.Models;

namespace RaceDay.Web.ViewModels;

public class MyEnrolmentsViewModel
{
    public List<MyEnrolment> Enrolments { get; set; } = new();
    public List<MyEnrolment> Upcoming => Enrolments
        .Where(e => e.EventDate.Date >= DateTime.Today)
        .OrderBy(e => e.EventDate)
        .ToList();
    public List<MyEnrolment> Past => Enrolments
        .Where(e => e.EventDate.Date < DateTime.Today)
        .OrderByDescending(e => e.EventDate)
        .ToList();
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [MaxLength(100, ErrorMessage = "Your name can be at most 100 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime MemberSince { get; set; }
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int? StatusCode { get; set; }
    public string Title { get; set; } = "Something went wrong";
    public string Message { get; set; } = "An unexpected error occurred. Please try again.";
}
