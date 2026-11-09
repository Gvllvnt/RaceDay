using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[RequireLogin("Organiser")]
public class OrganiserController : AppController
{
    private readonly IRaceDayApiClient _api;

    public OrganiserController(IRaceDayApiClient api) => _api = api;

    // Dashboard: summary of the organiser's events, enrolments per event, upcoming dates.
    public async Task<IActionResult> Dashboard()
    {
        var result = await _api.GetMyEventsAsync();
        if (SessionExpired(result) is { } expired) return expired;

        if (!result.Success) TempData["Error"] = result.Error;

        return View(new DashboardViewModel { Events = result.Data ?? new() });
    }
}
