using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[RequireLogin]
public class ProfileController : AppController
{
    private readonly IRaceDayApiClient _api;

    public ProfileController(IRaceDayApiClient api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetProfileAsync();
        if (SessionExpired(result) is { } expired) return expired;

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "We couldn't load your profile.";
            return HomeFor(CurrentUser!);
        }

        return View(ToViewModel(result.Data));
    }

    [HttpPost]
    public async Task<IActionResult> Update(ProfileViewModel model)
    {
        var current = CurrentUser!;

        // Role and picture are read-only on this form; keep them for redisplay.
        model.Role = current.Role;
        model.ProfilePictureUrl = current.ProfilePictureUrl;

        if (!ModelState.IsValid) return View("Index", model);

        var result = await _api.UpdateProfileAsync(new UpdateProfileRequest
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim()
        });
        if (SessionExpired(result) is { } expired) return expired;

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We couldn't update your profile.");
            return View("Index", model);
        }

        current.FullName = result.Data.FullName;
        current.Email = result.Data.Email;
        HttpContext.Session.SetUser(current);

        TempData["Success"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UploadPicture(IFormFile? picture)
    {
        var problem = ImageUploadValidator.Validate(picture);
        if (problem != null)
        {
            TempData["Error"] = problem;
            return RedirectToAction(nameof(Index));
        }

        var result = await _api.UploadProfilePictureAsync(picture!);
        if (SessionExpired(result) is { } expired) return expired;

        if (!result.Success || result.Data?.ProfilePictureUrl == null)
        {
            TempData["Error"] = result.Error ?? "We couldn't upload your picture.";
            return RedirectToAction(nameof(Index));
        }

        var current = CurrentUser!;
        current.ProfilePictureUrl = result.Data.ProfilePictureUrl;
        HttpContext.Session.SetUser(current);

        TempData["Success"] = "Your profile picture has been updated.";
        return RedirectToAction(nameof(Index));
    }

    private static ProfileViewModel ToViewModel(ProfileInfo p) => new()
    {
        FullName = p.FullName,
        Email = p.Email,
        Role = p.Role,
        ProfilePictureUrl = p.ProfilePictureUrl,
        MemberSince = p.CreatedAt
    };
}
