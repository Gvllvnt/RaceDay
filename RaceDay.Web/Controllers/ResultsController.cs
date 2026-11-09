using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[RequireLogin("Participant")]
public class ResultsController : AppController
{
    private readonly IRaceDayApiClient _api;

    public ResultsController(IRaceDayApiClient api) => _api = api;

    // "My Results": personal race history, published by the organiser after each event.
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetMyResultsAsync(CurrentUser!.UserId);
        if (SessionExpired(result) is { } expired) return expired;

        if (!result.Success) TempData["Error"] = result.Error;

        return View(result.Data ?? new List<MyResult>());
    }
}
