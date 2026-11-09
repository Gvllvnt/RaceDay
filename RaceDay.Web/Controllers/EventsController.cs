using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class EventsController : AppController
{
    private readonly IRaceDayApiClient _api;

    public EventsController(IRaceDayApiClient api) => _api = api;

    // Public: anyone can read the full event information and categories.
    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0) return NotFound();

        var result = await _api.GetEventAsync(id);
        if (result.IsNotFound) return NotFound();

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "We couldn't load that event.";
            return RedirectToAction("Index", "Home");
        }

        var model = new EventDetailsViewModel { Event = result.Data };

        // For logged-in participants, work out which categories they have already entered.
        if (CurrentUser is { IsParticipant: true } user)
        {
            var mine = await _api.GetMyEnrolmentsAsync(user.UserId);
            if (SessionExpired(mine) is { } expired) return expired;

            if (mine.Success && mine.Data != null)
                model.EnrolledCategoryIds = mine.Data.Select(e => e.CategoryId).ToHashSet();
        }

        return View(model);
    }
}
