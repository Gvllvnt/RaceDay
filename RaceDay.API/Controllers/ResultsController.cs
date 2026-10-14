using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Results;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context) => _context = context;

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(ClaimTypes.Role)!.Value;

    /// <summary>Add result for an enrolment (Organiser owner only)</summary>
    [HttpPost("enrolments/{enrolmentId}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int enrolmentId, [FromBody] CreateResultDto dto)
    {
        var enrolment = await _context.Enrolments
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

        if (enrolment == null) return NotFound(new { message = "Enrolment not found" });
        if (enrolment.Category!.Event!.OrganiserId != CurrentUserId) return Forbid();

        if (enrolment.Result != null)
            return Conflict(new { message = "Result already exists for this enrolment" });

        var result = new Result
        {
            EnrolmentId = enrolmentId,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            Pace = dto.Pace
        };

        _context.Results.Add(result);
        enrolment.Status = "Completed";
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOne), new { id = result.ResultId }, result);
    }

    /// <summary>Update result (Organiser owner only)</summary>
    [HttpPut("results/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateResultDto dto)
    {
        var result = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null) return NotFound();
        if (result.Enrolment!.Category!.Event!.OrganiserId != CurrentUserId) return Forbid();

        result.FinishTime = dto.FinishTime;
        result.Position = dto.Position;
        result.Pace = dto.Pace;

        await _context.SaveChangesAsync();
        return Ok(result);
    }

    /// <summary>Get single result</summary>
    [HttpGet("results/{id}")]
    [Authorize]
    public async Task<IActionResult> GetOne(int id)
    {
        var result = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null) return NotFound();

        if (CurrentRole == "Participant" && result.Enrolment!.ParticipantId != CurrentUserId)
            return Forbid();

        return Ok(new
        {
            result.ResultId,
            result.FinishTime,
            result.Position,
            result.Pace,
            Participant = result.Enrolment!.Participant!.FullName,
            Event = result.Enrolment.Category!.Event!.Title,
            Category = result.Enrolment.Category.Name
        });
    }

    /// <summary>Get all results for an event (Organiser owner only)</summary>
    [HttpGet("events/{eventId}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var evt = await _context.Events.FindAsync(eventId);
        if (evt == null) return NotFound();
        if (evt.OrganiserId != CurrentUserId) return Forbid();

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .Where(r => r.Enrolment!.Category!.EventId == eventId)
            .Select(r => new
            {
                r.ResultId,
                r.FinishTime,
                r.Position,
                r.Pace,
                Participant = r.Enrolment!.Participant!.FullName,
                Category = r.Enrolment.Category!.Name
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Get participant's own result history</summary>
    [HttpGet("participants/{id}/results")]
    [Authorize]
    public async Task<IActionResult> GetForParticipant(int id)
    {
        if (CurrentRole == "Participant" && CurrentUserId != id)
            return Forbid();

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .Where(r => r.Enrolment!.ParticipantId == id)
            .OrderByDescending(r => r.Enrolment!.Category!.Event!.EventDate)
            .Select(r => new
            {
                r.ResultId,
                r.FinishTime,
                r.Position,
                r.Pace,
                Event = r.Enrolment!.Category!.Event!.Title,
                EventDate = r.Enrolment.Category.Event.EventDate,
                Category = r.Enrolment.Category.Name,
                Distance = r.Enrolment.Category.DistanceKm
            })
            .ToListAsync();

        return Ok(results);
    }
}