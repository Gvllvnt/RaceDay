using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[RequireLogin("Organiser")]
public class CategoryManagementController : AppController
{
    private readonly IRaceDayApiClient _api;

    public CategoryManagementController(IRaceDayApiClient api) => _api = api;

    [HttpGet]
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

        return View(new CategoryManagementViewModel
        {
            Event = evt.Data,
            NewCategory = new CategoryFormViewModel { EventId = eventId }
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Redisplay the page with the validation messages beside the add form.
            var evt = await _api.GetEventAsync(model.EventId);
            if (SessionExpired(evt) is { } expired) return expired;
            if (!evt.Success || evt.Data == null) return RedirectToAction("Index", "EventManagement");

            return View("Index", new CategoryManagementViewModel { Event = evt.Data, NewCategory = model });
        }

        var result = await _api.CreateCategoryAsync(model.EventId, ToRequest(model));
        if (SessionExpired(result) is { } gone) return gone;

        if (result.Success) TempData["Success"] = $"Category \"{model.Name}\" added.";
        else TempData["Error"] = result.Error ?? "We couldn't add that category.";

        return RedirectToAction(nameof(Index), new { eventId = model.EventId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, int eventId)
    {
        if (id <= 0 || eventId <= 0) return NotFound();

        var evt = await _api.GetEventAsync(eventId);
        if (SessionExpired(evt) is { } expired) return expired;
        if (!evt.Success || evt.Data == null) return NotFound();

        var category = evt.Data.Categories.FirstOrDefault(c => c.CategoryId == id);
        if (category == null) return NotFound();

        ViewBag.EventTitle = evt.Data.Title;
        return View(new CategoryFormViewModel
        {
            CategoryId = category.CategoryId,
            EventId = eventId,
            Name = category.Name,
            DistanceKm = category.DistanceKm,
            StartTime = category.StartTime,
            EntryFee = category.EntryFee,
            MaxParticipants = category.MaxParticipants
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        model.CategoryId = id;
        if (!ModelState.IsValid) return View(model);

        var result = await _api.UpdateCategoryAsync(id, ToRequest(model));
        if (SessionExpired(result) is { } expired) return expired;

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We couldn't save your changes.");
            return View(model);
        }

        TempData["Success"] = "Category updated.";
        return RedirectToAction(nameof(Index), new { eventId = model.EventId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        var result = await _api.DeleteCategoryAsync(id);
        if (SessionExpired(result) is { } expired) return expired;

        if (result.Success) TempData["Success"] = "Category deleted.";
        else TempData["Error"] = result.Error ?? "We couldn't delete that category.";

        return RedirectToAction(nameof(Index), new { eventId });
    }

    private static CategoryRequest ToRequest(CategoryFormViewModel m) => new()
    {
        Name = m.Name.Trim(),
        DistanceKm = m.DistanceKm!.Value,
        StartTime = m.StartTime!.Value,
        EntryFee = m.EntryFee!.Value,
        MaxParticipants = m.MaxParticipants!.Value
    };
}
