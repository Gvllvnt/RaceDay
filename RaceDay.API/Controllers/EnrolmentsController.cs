using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Enrolments;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrolmentsController(RaceDayDbContext context) => _context = context;

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(ClaimTypes.Role)!.Value;

    /// <summary>Participant enrols in an event category</summary>
    [HttpPost("events/{eventId}/enrol")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(int eventId, [FromBody] EnrolDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Check event exists
        var evt = await _context.Events.FindAsync(eventId);
        if (evt == null || !evt.IsActive)
            return NotFound(new { message = "Event not found" });

        // Check category belongs to this event
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId && c.EventId == eventId);
        if (category == null)
            return NotFound(new { message = "Category not found for this event" });

        // Check already enrolled
        var existing = await _context.Enrolments
            .AnyAsync(e => e.ParticipantId == CurrentUserId && e.CategoryId == dto.CategoryId);
        if (existing)
            return Conflict(new { message = "You are already enrolled in this category" });

        // Check capacity
        var enrolCount = await _context.Enrolments.CountAsync(e => e.CategoryId == dto.CategoryId);
        if (enrolCount >= category.MaxParticipants)
            return BadRequest(new { message = "Category is full" });

        // Generate unique race number
        var raceNumber = await GenerateRaceNumber();

        var enrolment = new Enrolment
        {
            ParticipantId = CurrentUserId,
            CategoryId = dto.CategoryId,
            RaceNumber = raceNumber,
            Status = "Registered"
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOne), new { id = enrolment.EnrolmentId }, new
        {
            enrolment.EnrolmentId,
            enrolment.RaceNumber,
            enrolment.Status,
            enrolment.EnrolledAt,
            Event = evt.Title,
            Category = category.Name
        });
    }

    /// <summary>Get specific enrolment - Participant own or Organiser</summary>
    [HttpGet("enrolments/{id}")]
    [Authorize]
    public async Task<IActionResult> GetOne(int id)
    {
        var enrolment = await _context.Enrolments
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .Include(e => e.Participant)
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment == null) return NotFound();

        if (CurrentRole == "Participant" && enrolment.ParticipantId != CurrentUserId)
            return Forbid();

        return Ok(new
        {
            enrolment.EnrolmentId,
            enrolment.RaceNumber,
            enrolment.Status,
            enrolment.EnrolledAt,
            Participant = enrolment.Participant!.FullName,
            Event = enrolment.Category!.Event!.Title,
            Category = enrolment.Category.Name,
            Distance = enrolment.Category.DistanceKm,
            Result = enrolment.Result == null ? null : new
            {
                enrolment.Result.FinishTime,
                enrolment.Result.Position,
                enrolment.Result.Pace
            }
        });
    }

    /// <summary>Get all enrolments for an event (Organiser owner only)</summary>
    [HttpGet("events/{eventId}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var evt = await _context.Events.FindAsync(eventId);
        if (evt == null) return NotFound();
        if (evt.OrganiserId != CurrentUserId) return Forbid();

        var enrolments = await _context.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .Where(e => e.Category!.EventId == eventId)
            .Select(e => new
            {
                e.EnrolmentId,
                e.RaceNumber,
                e.Status,
                e.EnrolledAt,
                Participant = e.Participant!.FullName,
                ParticipantEmail = e.Participant.Email,
                Category = e.Category!.Name
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Update enrolment status (Organiser owner only)</summary>
    [HttpPut("enrolments/{id}/status")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var validStatuses = new[] { "Registered", "Completed", "DNS", "DNF" };
        if (!validStatuses.Contains(dto.Status))
            return BadRequest(new { message = "Invalid status. Must be Registered, Completed, DNS, or DNF" });

        var enrolment = await _context.Enrolments
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment == null) return NotFound();
        if (enrolment.Category!.Event!.OrganiserId != CurrentUserId) return Forbid();

        enrolment.Status = dto.Status;
        await _context.SaveChangesAsync();

        return Ok(new { enrolment.EnrolmentId, enrolment.Status });
    }

    /// <summary>Cancel enrolment (Participant own only)</summary>
    [HttpDelete("enrolments/{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var enrolment = await _context.Enrolments.FindAsync(id);
        if (enrolment == null) return NotFound();

        if (CurrentRole == "Participant" && enrolment.ParticipantId != CurrentUserId)
            return Forbid();

        _context.Enrolments.Remove(enrolment);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<int> GenerateRaceNumber()
    {
        var random = new Random();
        int raceNumber;
        bool exists;
        do
        {
            raceNumber = random.Next(10000, 99999);
            exists = await _context.Enrolments.AnyAsync(e => e.RaceNumber == raceNumber);
        } while (exists);

        return raceNumber;
    }
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}