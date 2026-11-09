using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;
using RaceDay.Web.Models;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class AccountController : AppController
{
    private readonly IRaceDayApiClient _api;

    public AccountController(IRaceDayApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (CurrentUser is { } user) return HomeFor(user);
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.LoginAsync(new LoginRequest
        {
            Email = model.Email.Trim(),
            Password = model.Password
        });

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty,
                result.IsUnauthorized ? "Incorrect email or password." : result.Error ?? "We couldn't log you in.");
            model.Password = string.Empty;
            return View(model);
        }

        await StartSessionAsync(result.Data);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return HomeFor(CurrentUser!);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (CurrentUser is { } user) return HomeFor(user);
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.RegisterAsync(new RegisterRequest
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Password = model.Password,
            Role = model.Role
        });

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty,
                result.StatusCode == System.Net.HttpStatusCode.Conflict
                    ? "An account with that email address already exists. Try logging in instead."
                    : result.Error ?? "We couldn't create your account.");
            model.Password = string.Empty;
            model.ConfirmPassword = string.Empty;
            return View(model);
        }

        await StartSessionAsync(result.Data);
        TempData["Success"] = $"Welcome to RaceDay, {result.Data.FullName}!";
        return HomeFor(CurrentUser!);
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out. See you at the start line!";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task StartSessionAsync(AuthResponse auth)
    {
        var user = new SessionUser
        {
            UserId = auth.UserId,
            FullName = auth.FullName,
            Email = auth.Email,
            Role = auth.Role,
            Token = auth.Token,
            ExpiresAtUtc = auth.ExpiresAt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(auth.ExpiresAt, DateTimeKind.Utc)
                : auth.ExpiresAt.ToUniversalTime()
        };
        HttpContext.Session.SetUser(user);

        // The token is now in the session, so we can fetch the profile picture for the nav bar.
        var profile = await _api.GetProfileAsync();
        if (profile.Success && profile.Data != null)
        {
            user.ProfilePictureUrl = profile.Data.ProfilePictureUrl;
            HttpContext.Session.SetUser(user);
        }
    }
}
