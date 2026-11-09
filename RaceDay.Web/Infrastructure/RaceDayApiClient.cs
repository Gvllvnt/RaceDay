using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Infrastructure;

public class RaceDayApiClient : IRaceDayApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<RaceDayApiClient> _logger;

    public RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor, ILogger<RaceDayApiClient> logger)
    {
        _http = http;
        _accessor = accessor;
        _logger = logger;
    }

    // ---------------- Auth ----------------

    public Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request)
        => SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/login", request);

    public Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request)
        => SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/register", request);

    // ---------------- Events ----------------

    public Task<ApiResult<List<EventSummary>>> GetEventsAsync(string? search, string? eventType, string? location, bool upcomingOnly)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add("search=" + Uri.EscapeDataString(search.Trim()));
        if (!string.IsNullOrWhiteSpace(eventType)) query.Add("eventType=" + Uri.EscapeDataString(eventType));
        if (!string.IsNullOrWhiteSpace(location)) query.Add("location=" + Uri.EscapeDataString(location.Trim()));
        if (upcomingOnly) query.Add("upcomingOnly=true");

        var url = "api/events" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        return SendAsync<List<EventSummary>>(HttpMethod.Get, url);
    }

    public Task<ApiResult<EventDetail>> GetEventAsync(int eventId)
        => SendAsync<EventDetail>(HttpMethod.Get, $"api/events/{eventId}");

    public Task<ApiResult<List<EventSummary>>> GetMyEventsAsync()
        => SendAsync<List<EventSummary>>(HttpMethod.Get, "api/events/mine");

    public Task<ApiResult<EventDetail>> CreateEventAsync(EventRequest request)
        => SendAsync<EventDetail>(HttpMethod.Post, "api/events", request);

    public async Task<ApiResult> UpdateEventAsync(int eventId, EventRequest request)
        => await SendAsync<object>(HttpMethod.Put, $"api/events/{eventId}", request);

    public async Task<ApiResult> DeleteEventAsync(int eventId)
        => await SendAsync<object>(HttpMethod.Delete, $"api/events/{eventId}");

    public Task<ApiResult<ImageUploaded>> UploadEventBannerAsync(int eventId, IFormFile file)
        => SendAsync<ImageUploaded>(HttpMethod.Post, $"api/events/{eventId}/banner", content: BuildFileContent(file));

    // ---------------- Categories ----------------

    public async Task<ApiResult> CreateCategoryAsync(int eventId, CategoryRequest request)
        => await SendAsync<object>(HttpMethod.Post, $"api/events/{eventId}/categories", request);

    public async Task<ApiResult> UpdateCategoryAsync(int categoryId, CategoryRequest request)
        => await SendAsync<object>(HttpMethod.Put, $"api/categories/{categoryId}", request);

    public async Task<ApiResult> DeleteCategoryAsync(int categoryId)
        => await SendAsync<object>(HttpMethod.Delete, $"api/categories/{categoryId}");

    // ---------------- Enrolments ----------------

    public Task<ApiResult<EnrolmentCreated>> EnrolAsync(int eventId, int categoryId)
        => SendAsync<EnrolmentCreated>(HttpMethod.Post, $"api/events/{eventId}/enrol", new { categoryId });

    public Task<ApiResult<List<MyEnrolment>>> GetMyEnrolmentsAsync(int userId)
        => SendAsync<List<MyEnrolment>>(HttpMethod.Get, $"api/users/{userId}/enrolments");

    public Task<ApiResult<List<EventEnrolment>>> GetEventEnrolmentsAsync(int eventId)
        => SendAsync<List<EventEnrolment>>(HttpMethod.Get, $"api/events/{eventId}/enrolments");

    public async Task<ApiResult> UpdateEnrolmentStatusAsync(int enrolmentId, string status)
        => await SendAsync<object>(HttpMethod.Put, $"api/enrolments/{enrolmentId}/status", new { status });

    public async Task<ApiResult> CancelEnrolmentAsync(int enrolmentId)
        => await SendAsync<object>(HttpMethod.Delete, $"api/enrolments/{enrolmentId}");

    // ---------------- Results ----------------

    public async Task<ApiResult> CreateResultAsync(int enrolmentId, ResultRequest request)
        => await SendAsync<object>(HttpMethod.Post, $"api/enrolments/{enrolmentId}/results", request);

    public async Task<ApiResult> UpdateResultAsync(int resultId, ResultRequest request)
        => await SendAsync<object>(HttpMethod.Put, $"api/results/{resultId}", request);

    public Task<ApiResult<List<MyResult>>> GetMyResultsAsync(int userId)
        => SendAsync<List<MyResult>>(HttpMethod.Get, $"api/participants/{userId}/results");

    // ---------------- Profile ----------------

    public Task<ApiResult<ProfileInfo>> GetProfileAsync()
        => SendAsync<ProfileInfo>(HttpMethod.Get, "api/users/profile");

    public Task<ApiResult<ProfileInfo>> UpdateProfileAsync(UpdateProfileRequest request)
        => SendAsync<ProfileInfo>(HttpMethod.Put, "api/users/profile", request);

    public Task<ApiResult<ImageUploaded>> UploadProfilePictureAsync(IFormFile file)
        => SendAsync<ImageUploaded>(HttpMethod.Post, "api/users/profile/picture", content: BuildFileContent(file));

    // ---------------- Plumbing ----------------

    private static MultipartFormDataContent BuildFileContent(IFormFile file)
    {
        var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(file.OpenReadStream());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        form.Add(fileContent, "file", file.FileName);
        return form;
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body = null, HttpContent? content = null)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);

            var token = _accessor.HttpContext?.Session.GetUser()?.Token;
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (content != null)
                request.Content = content;
            else if (body != null)
                request.Content = JsonContent.Create(body, body.GetType(), null, JsonOptions);

            using var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return new ApiResult<T>
                {
                    Success = false,
                    StatusCode = response.StatusCode,
                    Error = ParseError(response.StatusCode, errorBody)
                };
            }

            if (typeof(T) == typeof(object) || response.StatusCode == HttpStatusCode.NoContent)
                return new ApiResult<T> { Success = true, StatusCode = response.StatusCode };

            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return new ApiResult<T> { Success = true, StatusCode = response.StatusCode, Data = data };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "RaceDay API request failed: {Method} {Url}", method, url);
            return Unavailable<T>();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "RaceDay API request timed out: {Method} {Url}", method, url);
            return Unavailable<T>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "RaceDay API returned unreadable data: {Method} {Url}", method, url);
            return new ApiResult<T>
            {
                Success = false,
                StatusCode = HttpStatusCode.BadGateway,
                Error = "We received an unexpected response from the RaceDay service. Please try again."
            };
        }
    }

    private static ApiResult<T> Unavailable<T>() => new()
    {
        Success = false,
        StatusCode = HttpStatusCode.ServiceUnavailable,
        Error = "We couldn't reach the RaceDay service right now. Please try again in a moment."
    };

    /// <summary>Turns an API error body into a short, user-friendly sentence.</summary>
    private static string ParseError(HttpStatusCode status, string body)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                        return message.GetString() ?? Fallback(status);

                    if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        var messages = new List<string>();
                        foreach (var field in errors.EnumerateObject())
                        {
                            if (field.Value.ValueKind != JsonValueKind.Array) continue;
                            foreach (var item in field.Value.EnumerateArray())
                            {
                                var text = item.GetString();
                                if (!string.IsNullOrWhiteSpace(text) && !text.Contains("JSON value", StringComparison.OrdinalIgnoreCase))
                                    messages.Add(text);
                            }
                        }
                        if (messages.Count > 0) return string.Join(" ", messages);
                        return "Some of the information entered was not valid. Please check it and try again.";
                    }
                }
            }
            catch (JsonException)
            {
                // Not JSON - fall through to the generic message.
            }
        }

        return Fallback(status);
    }

    private static string Fallback(HttpStatusCode status) => status switch
    {
        HttpStatusCode.BadRequest => "Some of the information entered was not valid. Please check it and try again.",
        HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
        HttpStatusCode.Forbidden => "You don't have permission to do that.",
        HttpStatusCode.NotFound => "We couldn't find what you were looking for.",
        HttpStatusCode.Conflict => "That conflicts with something that already exists.",
        _ => "Something went wrong on our side. Please try again."
    };
}
