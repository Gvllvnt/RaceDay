using System.Text.Json;

namespace RaceDay.Web.Infrastructure;

/// <summary>The authenticated user as stored in the session.</summary>
public class SessionUser
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public bool IsOrganiser => Role == "Organiser";
    public bool IsParticipant => Role == "Participant";
}

public static class UserSessionExtensions
{
    private const string Key = "raceday.user";

    public static void SetUser(this ISession session, SessionUser user)
        => session.SetString(Key, JsonSerializer.Serialize(user));

    /// <summary>Returns the logged-in user, or null if there is none or the token has expired.</summary>
    public static SessionUser? GetUser(this ISession session)
    {
        var json = session.GetString(Key);
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            var user = JsonSerializer.Deserialize<SessionUser>(json);
            if (user == null || user.ExpiresAtUtc <= DateTime.UtcNow)
            {
                session.Remove(Key);
                return null;
            }
            return user;
        }
        catch (JsonException)
        {
            session.Remove(Key);
            return null;
        }
    }
}
