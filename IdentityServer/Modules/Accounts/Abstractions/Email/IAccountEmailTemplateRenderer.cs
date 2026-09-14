using IdentityServer.Modules.Accounts.Contracts.Email;

namespace IdentityServer.Modules.Accounts.Abstractions.Email
{
    /// <summary>Renders localized account-email content inside the shared HTML and plain-text layouts.</summary>
    internal interface IAccountEmailTemplateRenderer
    {
        /// <summary>Renders an account-confirmation email.</summary>
        /// <param name="username">The display name included in the message.</param>
        /// <param name="code">The separate six-digit confirmation code.</param>
        /// <param name="actionUrl">The one-time account-confirmation URL.</param>
        /// <param name="expiresUtc">The UTC time at which the challenge expires.</param>
        /// <param name="culture">The preferred content culture, when supplied.</param>
        /// <returns>The rendered subject, HTML body, and plain-text body.</returns>
        AccountEmailContent RenderConfirmation(
            string username,
            string code,
            string actionUrl,
            DateTime expiresUtc,
            string? culture);

        /// <summary>Renders a password-recovery email.</summary>
        /// <param name="username">The display name included in the message.</param>
        /// <param name="code">The separate six-digit password-recovery code.</param>
        /// <param name="actionUrl">The one-time password-recovery URL.</param>
        /// <param name="expiresUtc">The UTC time at which the challenge expires.</param>
        /// <param name="culture">The preferred content culture, when supplied.</param>
        /// <returns>The rendered subject, HTML body, and plain-text body.</returns>
        AccountEmailContent RenderPasswordRecovery(
            string username,
            string code,
            string actionUrl,
            DateTime expiresUtc,
            string? culture);
    }
}
