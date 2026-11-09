using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[RequireLogin("Organiser")]
public class EnrolmentManagementController : AppController
{
    private readonly IRaceDayApiClient _api;

    public EnrolmentManagementController(IRaceDayApiClient api) => _api = api;

    // Every participant enrolled in one event, with their category and status.
    public async Task<IActionResult> Index(int eventId, string? category)
    {
        if (eventId <= 0) return NotFound();

        var evt = await _api.GetEventAsync(eventId);
        if (SessionExpired(evt) is { } expired) return expired;
        if (evt.IsNotFound) return NotFound();

        if (!evt.Success || evt.Data == null)
        {
            TempData["Error"] = evt.Error ?? "We couldn't load that event.";
            return RedirectToAction("Index", "EventManagement");
        }

        var enrolments = await _api.GetEventEnrolmentsAsync(eventId);
        if (SessionExpired(enrolments) is { } gone) return gone;

        if (!enrolments.Success)
        {
            // Most likely "not your event" - the API enforces ownership.
            TempData["Error"] = enrolments.Error ?? "We couldn't load the enrolments.";
            return RedirectToAction("Index", "EventManagement");
        }

        var list = enrolments.Data ?? new();
        if (!string.IsNullOrWhiteSpace(category))
            list = list.Where(e => e.Category == category).ToList();

        return View(new EnrolmentManagementViewModel
        {
            Event = evt.Data,
            Enrolments = list.OrderBy(e => e.Category).ThenBy(e => e.Participant).ToList(),
            CategoryFilter = category
        });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int enrolmentId, int eventId, string status, string? category)
    {
        if (!EnrolmentManagementViewModel.Statuses.Contains(status))
        {
            TempData["Error"] = "Please choose a valid status.";
            return RedirectToAction(nameof(Index), new { eventId, category });
        }

        var result = await _api.UpdateEnrolmentStatusAsync(enrolmentId, status);
        if (SessionExpired(result) is { } expired) return expired;

        if (result.Success) TempData["Success"] = $"Enrolment marked as {status}.";
        else TempData["Error"] = result.Error ?? "We couldn't update that enrolment.";

        return RedirectToAction(nameof(Index), new { eventId, category });
    }
}
