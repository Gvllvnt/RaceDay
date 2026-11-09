using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Users;
using RaceDay.API.Services;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly IBlobStorageService _blobStorage;

    public UsersController(RaceDayDbContext context, IBlobStorageService blobStorage)
    {
        _context = context;
        _blobStorage = blobStorage;
    }

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(ClaimTypes.Role)!.Value;

    /// <summary>Get current user's profile</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _context.Users
            .Where(u => u.UserId == CurrentUserId)
            .Select(u => new { u.UserId, u.Email, u.FullName, u.Role, u.ProfilePictureUrl, u.CreatedAt })
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
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailTaken = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.UserId != user.UserId);
            if (emailTaken)
                return Conflict(new { message = "That email address is already in use." });

            user.Email = dto.Email;
        }

        await _context.SaveChangesAsync();
        return Ok(new { user.UserId, user.Email, user.FullName, user.Role, user.ProfilePictureUrl });
    }

    /// <summary>Upload or replace the current user's profile picture in Azure Blob Storage</summary>
    [HttpPost("profile/picture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        var validationError = _blobStorage.ValidateImage(file);
        if (validationError != null)
            return BadRequest(new { message = validationError });

        try
        {
            var oldUrl = user.ProfilePictureUrl;
            user.ProfilePictureUrl = await _blobStorage.UploadImageAsync(file, "profiles");
            await _context.SaveChangesAsync();
            await _blobStorage.DeleteAsync(oldUrl);

            return Ok(new { profilePictureUrl = user.ProfilePictureUrl });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is Azure.RequestFailedException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Image storage is currently unavailable. Please try again later." });
        }
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
                e.CategoryId,
                EventId = e.Category!.EventId,
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
