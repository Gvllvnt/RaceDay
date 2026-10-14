using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Events;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context) => _context = context;

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Get all active events - public with optional filters</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? location, [FromQuery] string? eventType)
    {
        var q = _context.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .Where(e => e.IsActive);

        if (!string.IsNullOrEmpty(location))
            q = q.Where(e => e.Location.Contains(location));

        if (!string.IsNullOrEmpty(eventType))
            q = q.Where(e => e.EventType == eventType);

        var events = await q.Select(e => new
        {
            e.EventId,
            e.Title,
            e.Description,
            e.EventDate,
            e.Location,
            e.Distance,
            e.EventType,
            e.RouteInfo,
            Organiser = e.Organiser!.FullName,
            CategoryCount = e.Categories.Count
        }).ToListAsync();

        return Ok(events);
    }

    /// <summary>Get a single event with its categories - public</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int id)
    {
        var evt = await _context.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventId == id && e.IsActive);

        if (evt == null) return NotFound();

        return Ok(new
        {
            evt.EventId,
            evt.Title,
            evt.Description,
            evt.EventDate,
            evt.Location,
            evt.Distance,
            evt.EventType,
            evt.RouteInfo,
            evt.WeatherInfo,
            Organiser = new { evt.Organiser!.FullName, evt.Organiser.Email },
            Categories = evt.Categories.Select(c => new
            {
                c.CategoryId,
                c.Name,
                c.DistanceKm,
                c.StartTime,
                c.EntryFee,
                c.MaxParticipants,
                CurrentEnrolments = _context.Enrolments.Count(en => en.CategoryId == c.CategoryId)
            })
        });
    }

    /// <summary>Create a new event (Organiser only)</summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var evt = new Event
        {
            OrganiserId = CurrentUserId,
            Title = dto.Title,
            Description = dto.Description,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Distance = dto.Distance,
            EventType = dto.EventType,
            RouteInfo = dto.RouteInfo,
            WeatherInfo = dto.WeatherInfo
        };

        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = evt.EventId }, evt);
    }

    /// <summary>Update an event (Organiser owner only)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt == null) return NotFound();
        if (evt.OrganiserId != CurrentUserId) return Forbid();

        evt.Title = dto.Title ?? evt.Title;
        evt.Description = dto.Description ?? evt.Description;
        evt.EventDate = dto.EventDate ?? evt.EventDate;
        evt.Location = dto.Location ?? evt.Location;
        evt.Distance = dto.Distance ?? evt.Distance;
        evt.EventType = dto.EventType ?? evt.EventType;
        evt.RouteInfo = dto.RouteInfo ?? evt.RouteInfo;
        evt.WeatherInfo = dto.WeatherInfo ?? evt.WeatherInfo;
        evt.IsActive = dto.IsActive ?? evt.IsActive;

        await _context.SaveChangesAsync();
        return Ok(evt);
    }

    /// <summary>Soft delete event (Organiser owner only)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt == null) return NotFound();
        if (evt.OrganiserId != CurrentUserId) return Forbid();

        evt.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Get upcoming events in next 30 days - public</summary>
    [HttpGet("upcoming")]
    [AllowAnonymous]
    public async Task<IActionResult> Upcoming()
    {
        var cutoff = DateTime.UtcNow.AddDays(30);
        var events = await _context.Events
            .Where(e => e.IsActive && e.EventDate >= DateTime.UtcNow && e.EventDate <= cutoff)
            .OrderBy(e => e.EventDate)
            .Select(e => new { e.EventId, e.Title, e.EventDate, e.Location, e.EventType })
            .ToListAsync();
        return Ok(events);
    }

    /// <summary>Search events by title or location - public</summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Search query required" });

        var events = await _context.Events
            .Where(e => e.IsActive && (e.Title.Contains(q) || e.Location.Contains(q)))
            .Select(e => new { e.EventId, e.Title, e.EventDate, e.Location, e.EventType })
            .ToListAsync();

        return Ok(events);
    }
}
