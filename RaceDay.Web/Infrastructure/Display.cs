namespace RaceDay.Web.Infrastructure;

/// <summary>Small presentation helpers shared by the views.</summary>
public static class Display
{
    public static string Money(decimal amount) => amount == 0 ? "Free" : "R" + amount.ToString("N2");

    public static string Time(TimeOnly? time) => time.HasValue ? time.Value.ToString("HH:mm:ss") : "-";

    public static string StartTime(TimeOnly time) => time.ToString("HH:mm");

    public static string Distance(decimal? km) => km.HasValue ? km.Value.ToString("0.##") + " km" : "-";

    public static string Date(DateTime date) => date.ToString("ddd, d MMM yyyy");

    public static string EventTypeLabel(string? type) => type switch
    {
        "run" => "Run",
        "walk" => "Walk",
        "cycle" => "Cycle",
        _ => "Event"
    };

    public static string EventTypeIcon(string? type) => type switch
    {
        "run" => "bi-lightning-charge-fill",
        "walk" => "bi-tree-fill",
        "cycle" => "bi-bicycle",
        _ => "bi-flag-fill"
    };

    /// <summary>CSS class for an enrolment status badge (green = Confirmed, amber = Pending, ...).</summary>
    public static string StatusClass(string? status) => status switch
    {
        "Confirmed" => "status-confirmed",
        "Pending" => "status-pending",
        "Registered" => "status-pending",
        "Completed" => "status-completed",
        "DNF" => "status-dnf",
        _ => "status-dns"
    };

    public static string StatusIcon(string? status) => status switch
    {
        "Confirmed" => "bi-check-circle-fill",
        "Pending" => "bi-hourglass-split",
        "Registered" => "bi-hourglass-split",
        "Completed" => "bi-trophy-fill",
        "DNF" => "bi-x-octagon-fill",
        _ => "bi-dash-circle-fill"
    };

    public static string Initials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return "?";
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
    }

    public static string Ordinal(int n)
    {
        var lastTwo = n % 100;
        if (lastTwo >= 11 && lastTwo <= 13) return n + "th";
        switch (n % 10)
        {
            case 1: return n + "st";
            case 2: return n + "nd";
            case 3: return n + "rd";
            default: return n + "th";
        }
    }
}
