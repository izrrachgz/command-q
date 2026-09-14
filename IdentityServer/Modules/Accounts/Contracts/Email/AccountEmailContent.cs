namespace IdentityServer.Modules.Accounts.Contracts.Email
{
    /// <summary>Contains the fully rendered subject and bodies of an account email.</summary>
    /// <param name="Subject">The localized email subject.</param>
    /// <param name="HtmlBody">The complete HTML representation.</param>
    /// <param name="TextBody">The complete plain-text representation.</param>
    internal sealed record AccountEmailContent(string Subject, string HtmlBody, string TextBody);
}
