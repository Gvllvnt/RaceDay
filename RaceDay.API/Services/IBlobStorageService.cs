namespace RaceDay.API.Services;

/// <summary>
/// Abstraction over Azure Blob Storage. All image uploads for RaceDay
/// (event banners, profile pictures) go through this service so the MVC
/// application never talks to Azure directly.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>Returns an error message if the file is not an acceptable image, otherwise null.</summary>
    string? ValidateImage(IFormFile? file);

    /// <summary>Uploads the image into the given virtual folder and returns its public URL.</summary>
    Task<string> UploadImageAsync(IFormFile file, string folder);

    /// <summary>Best-effort removal of a previously uploaded blob (ignored if the URL is not ours).</summary>
    Task DeleteAsync(string? blobUrl);
}
