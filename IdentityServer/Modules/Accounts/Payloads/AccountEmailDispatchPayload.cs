namespace IdentityServer.Modules.Accounts.Payloads
{
    /// <summary>Represents a protected account email waiting to be submitted to the email API.</summary>
    /// <param name="RecipientAddress">The destination email address.</param>
    /// <param name="RecipientName">The destination display name.</param>
    /// <param name="Subject">The rendered message subject.</param>
    /// <param name="HtmlBody">The rendered HTML body.</param>
    /// <param name="TextBody">The rendered plain-text body.</param>
    /// <param name="ProfileName">The email delivery profile requested from the Email API.</param>
    /// <param name="IdempotencyKey">The stable key used to suppress duplicate queue submissions.</param>
    /// <param name="AccountReference">The non-secret account reference attached to the queued message.</param>
    internal sealed record AccountEmailDispatchPayload(
        string RecipientAddress,
        string RecipientName,
        string Subject,
        string HtmlBody,
        string TextBody,
        string? ProfileName,
        string IdempotencyKey,
        string AccountReference);
}
