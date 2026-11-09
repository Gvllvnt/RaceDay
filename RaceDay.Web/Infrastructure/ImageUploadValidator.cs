namespace RaceDay.Web.Infrastructure;

/// <summary>
/// Friendly pre-checks so users get instant feedback. The API re-validates
/// every upload, so this is a convenience rather than a security boundary.
/// </summary>
public static class ImageUploadValidator
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp" };

    public static string? Validate(IFormFile? file)
    {
        if (file == null || file.Length == 0) return "Please choose an image to upload.";
        if (file.Length > MaxBytes) return "The image must be 5 MB or smaller.";

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return "Only JPG, PNG or WEBP images are allowed.";

        return null;
    }
}
