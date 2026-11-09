using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[RequireLogin("Organiser")]
public class ResultManagementController : AppController
{
    private readonly IRaceDayApiClient _api;

    public ResultManagementController(IRaceDayApiClient api) => _api = api;

    // Capture and publish finish times and finishing positions after an event concludes.
    public async Task<IActionResult> Index(int eventId)
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
            TempData["Error"] = enrolments.Error ?? "We couldn't load the enrolments.";
            return RedirectToAction("Index", "EventManagement");
        }

        return View(new ResultManagementViewModel
        {
            Event = evt.Data,
            Enrolments = (enrolments.Data ?? new())
                .Where(e => e.Status != "DNS")
                .OrderBy(e => e.Position ?? int.MaxValue)
                .ThenBy(e => e.Category)
                .ThenBy(e => e.Participant)
                .ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Save(int eventId, int enrolmentId, int? resultId, TimeOnly? finishTime, int? position)
    {
        if (finishTime == null)
        {
            TempData["Error"] = "Please enter a valid finish time (hours:minutes:seconds).";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        if (position == null || position < 1 || position > 1000000)
        {
            TempData["Error"] = "Please enter a finishing position of 1 or higher.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var request = new ResultRequest { FinishTime = finishTime, Position = position };

        var result = resultId.HasValue
            ? await _api.UpdateResultAsync(resultId.Value, request)
            : await _api.CreateResultAsync(enrolmentId, request);

        if (SessionExpired(result) is { } expired) return expired;

        if (result.Success) TempData["Success"] = "Result published. The participant can now see it under My Results.";
        else TempData["Error"] = result.Error ?? "We couldn't save that result.";

        return RedirectToAction(nameof(Index), new { eventId });
    }
}
