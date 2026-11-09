using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[RequireLogin("Participant")]
public class EnrolmentsController : AppController
{
    private readonly IRaceDayApiClient _api;

    public EnrolmentsController(IRaceDayApiClient api) => _api = api;

    // "My enrolments": every event the participant has entered, with its status.
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetMyEnrolmentsAsync(CurrentUser!.UserId);
        if (SessionExpired(result) is { } expired) return expired;

        if (!result.Success)
            TempData["Error"] = result.Error;

        return View(new MyEnrolmentsViewModel { Enrolments = result.Data ?? new() });
    }

    [HttpPost]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (eventId <= 0 || categoryId <= 0)
        {
            TempData["Error"] = "Please choose a category to enrol in.";
            return RedirectToAction("Index", "Home");
        }

        var result = await _api.EnrolAsync(eventId, categoryId);
        if (SessionExpired(result) is { } expired) return expired;

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "We couldn't complete your enrolment.";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        TempData["Success"] = result.Data?.RaceNumber != null
            ? $"You're in! Your race number is {result.Data.RaceNumber}."
            : "You're in! Your enrolment has been recorded.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _api.CancelEnrolmentAsync(id);
        if (SessionExpired(result) is { } expired) return expired;

        if (result.Success) TempData["Success"] = "Your enrolment has been cancelled.";
        else TempData["Error"] = result.Error ?? "We couldn't cancel that enrolment.";

        return RedirectToAction(nameof(Index));
    }
}
