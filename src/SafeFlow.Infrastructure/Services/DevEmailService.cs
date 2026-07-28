using Microsoft.Extensions.Logging;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// Development-only placeholder implementation of <see cref="IEmailService"/>.
/// </summary>
/// <remarks>
/// <para>
/// Phase 1 does not integrate with an SMTP server, SendGrid, or AWS SES.
/// This implementation logs the email content at <c>Information</c> level so that
/// developers can observe email delivery in the console without external dependencies.
/// </para>
/// <para>
/// Replace this class with a production implementation (e.g., SendGrid adapter) in a
/// future phase by implementing <see cref="IEmailService"/> and updating the DI
/// registration in <c>DependencyInjection.cs</c>.
/// </para>
/// </remarks>
internal sealed class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;

    /// <summary>Initialises a new <see cref="DevEmailService"/>.</summary>
    public DevEmailService(ILogger<DevEmailService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task SendEmailAsync(
        string toAddress,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        IReadOnlyCollection<string>? ccAddresses = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[DEV-EMAIL] To: {To} | CC: {Cc} | Subject: {Subject}\n{Body}",
            toAddress,
            ccAddresses is { Count: > 0 } cc ? string.Join(", ", cc) : "(none)",
            subject,
            plainTextBody ?? "[html-only]");

        return Task.CompletedTask;
    }
}
