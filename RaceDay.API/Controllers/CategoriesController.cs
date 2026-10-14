using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Categories;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoriesController(RaceDayDbContext context) => _context = context;

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Get all categories for an event - public</summary>
    [HttpGet("events/{eventId}/categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var evtExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
        if (!evtExists) return NotFound(new { message = "Event not found" });

        var categories = await _context.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new
            {
                c.CategoryId,
                c.Name,
                c.DistanceKm,
                c.StartTime,
                c.EntryFee,
                c.MaxParticipants,
                CurrentEnrolments = _context.Enrolments.Count(e => e.CategoryId == c.CategoryId)
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>Add category to event (Organiser owner only)</summary>
    [HttpPost("events/{eventId}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var evt = await _context.Events.FindAsync(eventId);
        if (evt == null) return NotFound(new { message = "Event not found" });
        if (evt.OrganiserId != CurrentUserId) return Forbid();

        var category = new Category
        {
            EventId = eventId,
            Name = dto.Name,
            DistanceKm = dto.DistanceKm,
            StartTime = dto.StartTime,
            EntryFee = dto.EntryFee,
            MaxParticipants = dto.MaxParticipants
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetForEvent), new { eventId }, category);
    }

    /// <summary>Update category (Organiser owner only)</summary>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null) return NotFound();
        if (category.Event!.OrganiserId != CurrentUserId) return Forbid();

        category.Name = dto.Name;
        category.DistanceKm = dto.DistanceKm;
        category.StartTime = dto.StartTime;
        category.EntryFee = dto.EntryFee;
        category.MaxParticipants = dto.MaxParticipants;

        await _context.SaveChangesAsync();
        return Ok(category);
    }

    /// <summary>Delete category (Organiser owner only)</summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null) return NotFound();
        if (category.Event!.OrganiserId != CurrentUserId) return Forbid();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Get enrolment count for a category - public</summary>
    [HttpGet("categories/{id}/enrolments/count")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEnrolmentCount(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        var count = await _context.Enrolments.CountAsync(e => e.CategoryId == id);

        return Ok(new
        {
            CategoryId = id,
            Count = count,
            MaxParticipants = category.MaxParticipants,
            SpotsRemaining = category.MaxParticipants - count
        });
    }
}