namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Provides a provider-agnostic interface for sending transactional emails.
/// </summary>
/// <remarks>
/// <para>
/// The Infrastructure layer backs this interface with a concrete email provider
/// (SendGrid, SMTP, AWS SES, etc.) without exposing any provider SDK types to the
/// Application layer.
/// </para>
/// <para>
/// All email content should be prepared by the Application layer (subject, body,
/// recipient addresses) and handed to this interface.  HTML templates and plain-text
/// fallbacks are the caller's responsibility; this interface performs delivery only.
/// </para>
/// <para>
/// For high-volume scenarios, the Infrastructure implementation should enqueue the
/// message to an outbox or message queue rather than performing a synchronous HTTP
/// call, providing resilience against transient email-provider failures.
/// </para>
/// </remarks>
public interface IEmailService
{
    /// <summary>
    /// Sends a transactional email message.
    /// </summary>
    /// <param name="toAddress">
    /// The recipient's email address. Must be a valid RFC 5321 address.
    /// </param>
    /// <param name="subject">
    /// The email subject line. Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="htmlBody">
    /// The HTML-formatted body of the email. Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="plainTextBody">
    /// An optional plain-text alternative body for email clients that do not render
    /// HTML.  When <c>null</c>, no plain-text alternative is attached.
    /// </param>
    /// <param name="ccAddresses">
    /// An optional read-only collection of carbon-copy recipient addresses.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    Task SendEmailAsync(
        string toAddress,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        IReadOnlyCollection<string>? ccAddresses = null,
        CancellationToken cancellationToken = default);
}
