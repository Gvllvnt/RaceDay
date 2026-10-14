using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Users;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public UsersController(RaceDayDbContext context) => _context = context;

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(ClaimTypes.Role)!.Value;

    /// <summary>Get current user's profile</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _context.Users
            .Where(u => u.UserId == CurrentUserId)
            .Select(u => new { u.UserId, u.Email, u.FullName, u.Role, u.CreatedAt })
            .FirstOrDefaultAsync();

        return user == null ? NotFound() : Ok(user);
    }

    /// <summary>Update current user's profile</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
        if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;

        await _context.SaveChangesAsync();
        return Ok(new { user.UserId, user.Email, user.FullName, user.Role });
    }

    /// <summary>Get a participant's enrolments - Participant own only, Organiser any</summary>
    [HttpGet("{id}/enrolments")]
    public async Task<IActionResult> GetUserEnrolments(int id)
    {
        if (CurrentRole == "Participant" && CurrentUserId != id) return Forbid();

        var enrolments = await _context.Enrolments
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .Where(e => e.ParticipantId == id)
            .Select(e => new
            {
                e.EnrolmentId,
                e.RaceNumber,
                e.Status,
                e.EnrolledAt,
                Event = e.Category!.Event!.Title,
                Category = e.Category.Name,
                Distance = e.Category.DistanceKm,
                EventDate = e.Category.Event.EventDate
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Get all users (Organiser only)</summary>
    [HttpGet]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new { u.UserId, u.Email, u.FullName, u.Role, u.CreatedAt })
            .ToListAsync();
        return Ok(users);
    }
}
