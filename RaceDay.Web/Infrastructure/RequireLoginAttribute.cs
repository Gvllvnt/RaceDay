using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Infrastructure;

/// <summary>
/// Blocks unauthenticated users from protected pages (redirect to Login) and
/// users with the wrong role (redirect to Access Denied).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireLoginAttribute : Attribute, IActionFilter
{
    private readonly string? _role;

    public RequireLoginAttribute(string? role = null) => _role = role;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var http = context.HttpContext;
        var user = http.Session.GetUser();

        if (user == null)
        {
            string? returnUrl = http.Request.Method == "GET"
                ? http.Request.Path + http.Request.QueryString
                : null;

            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }

        if (_role != null && !string.Equals(user.Role, _role, StringComparison.Ordinal))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            return;
        }

        // Stop the browser back button from re-showing protected pages after logout.
        http.Response.Headers.CacheControl = "no-store, no-cache";
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
