using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[RequireLogin("Organiser")]
public class EventManagementController : AppController
{
    private readonly IRaceDayApiClient _api;

    public EventManagementController(IRaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetMyEventsAsync();
        if (SessionExpired(result) is { } expired) return expired;

        if (!result.Success) TempData["Error"] = result.Error;

        return View(result.Data ?? new List<EventSummary>());
    }

    [HttpGet]
    public IActionResult Create()
        => View("Form", new EventFormViewModel { EventDate = DateTime.Today.AddDays(30).AddHours(6) });

    [HttpPost]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (model.BannerFile != null)
        {
            var bannerProblem = ImageUploadValidator.Validate(model.BannerFile);
            if (bannerProblem != null) ModelState.AddModelError(nameof(model.BannerFile), bannerProblem);
        }

        if (!ModelState.IsValid) return View("Form", model);

        var created = await _api.CreateEventAsync(ToRequest(model));
        if (SessionExpired(created) is { } expired) return expired;

        if (!created.Success || created.Data == null)
        {
            ModelState.AddModelError(string.Empty, created.Error ?? "We couldn't create the event.");
            return View("Form", model);
        }

        var eventId = created.Data.EventId;

        // Banner goes to the API, which stores it in Azure Blob Storage.
        if (model.BannerFile != null)
        {
            var upload = await _api.UploadEventBannerAsync(eventId, model.BannerFile);
            if (!upload.Success)
            {
                TempData["Error"] = $"Your event was created, but the banner could not be uploaded: {upload.Error}";
                return RedirectToAction(nameof(Edit), new { id = eventId });
            }
        }

        TempData["Success"] = "Event created. Now add the categories people can enter.";
        return RedirectToAction("Index", "CategoryManagement", new { eventId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (id <= 0) return NotFound();

        var result = await _api.GetEventAsync(id);
        if (SessionExpired(result) is { } expired) return expired;
        if (result.IsNotFound) return NotFound();

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "We couldn't load that event.";
            return RedirectToAction(nameof(Index));
        }

        return View("Form", ToForm(result.Data));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        model.EventId = id;

        if (model.BannerFile != null)
        {
            var bannerProblem = ImageUploadValidator.Validate(model.BannerFile);
            if (bannerProblem != null) ModelState.AddModelError(nameof(model.BannerFile), bannerProblem);
        }

        if (!ModelState.IsValid) return View("Form", model);

        var updated = await _api.UpdateEventAsync(id, ToRequest(model));
        if (SessionExpired(updated) is { } expired) return expired;

        if (!updated.Success)
        {
            ModelState.AddModelError(string.Empty, updated.Error ?? "We couldn't save your changes.");
            return View("Form", model);
        }

        if (model.BannerFile != null)
        {
            var upload = await _api.UploadEventBannerAsync(id, model.BannerFile);
            if (!upload.Success)
            {
                TempData["Error"] = $"Your changes were saved, but the banner could not be uploaded: {upload.Error}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _api.DeleteEventAsync(id);
        if (SessionExpired(result) is { } expired) return expired;

        if (result.Success) TempData["Success"] = "Event deleted.";
        else TempData["Error"] = result.Error ?? "We couldn't delete that event.";

        return RedirectToAction(nameof(Index));
    }

    private static EventRequest ToRequest(EventFormViewModel m) => new()
    {
        Title = m.Title.Trim(),
        Description = m.Description?.Trim(),
        EventDate = m.EventDate!.Value,
        Location = m.Location.Trim(),
        Distance = m.Distance,
        EventType = m.EventType,
        RouteInfo = m.RouteInfo?.Trim(),
        WeatherInfo = m.WeatherInfo?.Trim()
    };

    private static EventFormViewModel ToForm(EventDetail e) => new()
    {
        EventId = e.EventId,
        Title = e.Title,
        Description = e.Description,
        EventDate = e.EventDate,
        Location = e.Location,
        Distance = e.Distance,
        EventType = e.EventType,
        RouteInfo = e.RouteInfo,
        WeatherInfo = e.WeatherInfo,
        ExistingBannerUrl = e.BannerUrl
    };
}
