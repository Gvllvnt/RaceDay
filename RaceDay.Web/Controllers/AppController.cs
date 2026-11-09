using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;

namespace RaceDay.Web.Controllers;

public abstract class AppController : Controller
{
    protected SessionUser? CurrentUser => HttpContext.Session.GetUser();

    /// <summary>
    /// If the API rejected our token (expired/invalid), end the session and send
    /// the user to the login page. Returns null when the result is not an auth failure.
    /// </summary>
    protected IActionResult? SessionExpired(ApiResult result)
    {
        if (!result.IsUnauthorized) return null;

        HttpContext.Session.Clear();
        TempData["Error"] = "Your session has expired. Please log in again.";
        return RedirectToAction("Login", "Account");
    }

    protected IActionResult HomeFor(SessionUser user)
        => user.IsOrganiser
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Index", "Home");
}
