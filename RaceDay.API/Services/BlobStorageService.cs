using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services;

public class BlobStorageService : IBlobStorageService
{
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".webp", "image/webp" }
    };

    private readonly IConfiguration _config;
    private readonly ILogger<BlobStorageService> _logger;
    private BlobContainerClient? _container;

    public BlobStorageService(IConfiguration config, ILogger<BlobStorageService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string? ValidateImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return "No image was uploaded.";

        if (file.Length > MaxBytes)
            return "The image must be 5 MB or smaller.";

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedTypes.ContainsKey(extension))
            return "Only JPG, PNG or WEBP images are allowed.";

        return null;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        var container = await GetContainerAsync();

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blobName = $"{folder}/{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = AllowedTypes[extension] }
        });

        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string? blobUrl)
    {
        if (string.IsNullOrWhiteSpace(blobUrl)) return;

        try
        {
            var container = await GetContainerAsync();
            var prefix = container.Uri.ToString().TrimEnd('/') + "/";
            if (!blobUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;

            var blobName = Uri.UnescapeDataString(blobUrl.Substring(prefix.Length));
            await container.DeleteBlobIfExistsAsync(blobName);
        }
        catch (Exception ex)
        {
            // Cleanup is best-effort and must never fail the request.
            _logger.LogWarning(ex, "Could not delete old blob {BlobUrl}", blobUrl);
        }
    }

    private async Task<BlobContainerClient> GetContainerAsync()
    {
        if (_container != null) return _container;

        var connectionString = _config["AzureBlob:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Azure Blob Storage is not configured.");

        var containerName = _config["AzureBlob:ContainerName"] ?? "raceday-images";

        var client = new BlobContainerClient(connectionString, containerName);
        await client.CreateIfNotExistsAsync(PublicAccessType.Blob);

        _container = client;
        return client;
    }
}
