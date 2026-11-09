using RaceDay.Web.Models;

namespace RaceDay.Web.Infrastructure;

/// <summary>
/// The ONLY way the MVC app reads or writes data: through the Part 2 RESTful API.
/// File uploads are also sent to the API, which stores them in Azure Blob Storage.
/// </summary>
public interface IRaceDayApiClient
{
    // Auth
    Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request);

    // Events
    Task<ApiResult<List<EventSummary>>> GetEventsAsync(string? search, string? eventType, string? location, bool upcomingOnly);
    Task<ApiResult<EventDetail>> GetEventAsync(int eventId);
    Task<ApiResult<List<EventSummary>>> GetMyEventsAsync();
    Task<ApiResult<EventDetail>> CreateEventAsync(EventRequest request);
    Task<ApiResult> UpdateEventAsync(int eventId, EventRequest request);
    Task<ApiResult> DeleteEventAsync(int eventId);
    Task<ApiResult<ImageUploaded>> UploadEventBannerAsync(int eventId, IFormFile file);

    // Categories
    Task<ApiResult> CreateCategoryAsync(int eventId, CategoryRequest request);
    Task<ApiResult> UpdateCategoryAsync(int categoryId, CategoryRequest request);
    Task<ApiResult> DeleteCategoryAsync(int categoryId);

    // Enrolments
    Task<ApiResult<EnrolmentCreated>> EnrolAsync(int eventId, int categoryId);
    Task<ApiResult<List<MyEnrolment>>> GetMyEnrolmentsAsync(int userId);
    Task<ApiResult<List<EventEnrolment>>> GetEventEnrolmentsAsync(int eventId);
    Task<ApiResult> UpdateEnrolmentStatusAsync(int enrolmentId, string status);
    Task<ApiResult> CancelEnrolmentAsync(int enrolmentId);

    // Results
    Task<ApiResult> CreateResultAsync(int enrolmentId, ResultRequest request);
    Task<ApiResult> UpdateResultAsync(int resultId, ResultRequest request);
    Task<ApiResult<List<MyResult>>> GetMyResultsAsync(int userId);

    // Profile
    Task<ApiResult<ProfileInfo>> GetProfileAsync();
    Task<ApiResult<ProfileInfo>> UpdateProfileAsync(UpdateProfileRequest request);
    Task<ApiResult<ImageUploaded>> UploadProfilePictureAsync(IFormFile file);
}
