using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class HomeController : AppController
{
    private readonly IRaceDayApiClient _api;

    public HomeController(IRaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? search, string? eventType, string? location)
    {
        // Organisers have their own landing page.
        if (CurrentUser is { IsOrganiser: true })
            return RedirectToAction("Dashboard", "Organiser");

        var result = await _api.GetEventsAsync(search, eventType, location, upcomingOnly: true);

        var model = new HomeIndexViewModel
        {
            Search = search,
            EventType = eventType,
            Location = location,
            Events = result.Data ?? new(),
            LoadError = result.Success ? null : result.Error
        };

        return View(model);
    }

    [IgnoreAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    [IgnoreAntiforgeryToken]
    public IActionResult Status(int code)
    {
        var model = new ErrorViewModel { StatusCode = code };

        switch (code)
        {
            case 404:
                model.Title = "Off the course";
                model.Message = "We couldn't find the page you were looking for.";
                break;
            case 403:
                model.Title = "Restricted area";
                model.Message = "You don't have access to that page.";
                break;
            default:
                model.Title = "Something went wrong";
                model.Message = "An unexpected error occurred. Please try again.";
                break;
        }

        Response.StatusCode = code;
        return View("Error", model);
    }
}
