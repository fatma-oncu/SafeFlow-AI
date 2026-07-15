namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Provides a storage-provider-agnostic interface for uploading, deleting, and
/// checking the existence of binary files.
/// </summary>
/// <remarks>
/// <para>
/// The Infrastructure layer may back this interface with any binary object store
/// (Azure Blob Storage, AWS S3, local disk, etc.) without exposing provider-specific
/// SDK types to the Application layer.
/// </para>
/// <para>
/// The <see cref="UploadAsync"/> method returns a unique, stable file key that
/// callers must persist (e.g., stored on the related domain entity) to reference the
/// file in subsequent <see cref="DeleteAsync"/> or <see cref="ExistsAsync"/> calls.
/// The key format is determined by the Infrastructure implementation (e.g., a
/// relative blob path such as <c>"incidents/2024/07/report.pdf"</c>).
/// </para>
/// <para>
/// Access-controlled file downloads (pre-signed URLs, SAS tokens) are outside the
/// scope of this interface and are handled by a separate read-side service to
/// preserve command/query separation.
/// </para>
/// </remarks>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file stream to the underlying storage provider.
    /// </summary>
    /// <param name="fileStream">
    /// The binary content to upload.  The stream must be readable and positioned at
    /// its beginning.  Callers retain ownership of the stream and are responsible
    /// for disposing it.
    /// </param>
    /// <param name="fileName">
    /// The original file name including extension (e.g., <c>"incident_photo.jpg"</c>).
    /// Used to set the content-disposition and to infer the content type when
    /// <paramref name="contentType"/> is not provided.
    /// </param>
    /// <param name="contentType">
    /// The MIME type of the file (e.g., <c>"image/jpeg"</c>, <c>"application/pdf"</c>).
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task whose result is the unique, stable file key assigned by the storage
    /// provider.  Persist this key on the related domain entity.
    /// </returns>
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently removes the file identified by <paramref name="fileKey"/> from the
    /// underlying storage provider.  A no-op if the key does not exist.
    /// </summary>
    /// <param name="fileKey">
    /// The unique file key returned by a prior <see cref="UploadAsync"/> call.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a file identified by <paramref name="fileKey"/> exists in
    /// the underlying storage provider.
    /// </summary>
    /// <param name="fileKey">
    /// The unique file key returned by a prior <see cref="UploadAsync"/> call.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task whose result is <c>true</c> if the file exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default);
}
