using Microsoft.Extensions.Logging;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// Local-disk implementation of <see cref="IFileStorageService"/> for development.
/// </summary>
/// <remarks>
/// <para>
/// Phase 1 does not integrate with Azure Blob Storage or AWS S3.
/// Files are written to a configurable base directory on the local filesystem,
/// allowing development and integration testing without cloud dependencies.
/// </para>
/// <para>
/// Replace this class with a cloud-backed implementation in a future phase.
/// The file key returned by <see cref="UploadAsync"/> is a relative path from
/// the base directory (e.g., <c>"uploads/2026/07/abc123.pdf"</c>).
/// </para>
/// </remarks>
internal sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    /// <summary>
    /// Initialises a new <see cref="LocalFileStorageService"/>.
    /// </summary>
    /// <param name="logger">Structured logger.</param>
    public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;

        // Default base path: current working directory / uploads
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(_basePath);
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        string datePath = Path.Combine(
            DateTime.UtcNow.Year.ToString(),
            DateTime.UtcNow.Month.ToString("D2"));

        string directory = Path.Combine(_basePath, datePath);
        Directory.CreateDirectory(directory);

        string uniqueFileName = $"{Guid.NewGuid():N}_{fileName}";
        string fullPath = Path.Combine(directory, uniqueFileName);

        await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(output, cancellationToken);

        // Return relative key (date/uniqueFileName)
        string key = Path.Combine(datePath, uniqueFileName).Replace('\\', '/');

        _logger.LogInformation(
            "[LOCAL-STORAGE] Uploaded {FileName} ({ContentType}) → {Key}",
            fileName, contentType, key);

        return key;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        string fullPath = Path.Combine(_basePath, fileKey.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("[LOCAL-STORAGE] Deleted {FileKey}", fileKey);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        string fullPath = Path.Combine(_basePath, fileKey.Replace('/', Path.DirectorySeparatorChar));
        return Task.FromResult(File.Exists(fullPath));
    }
}
