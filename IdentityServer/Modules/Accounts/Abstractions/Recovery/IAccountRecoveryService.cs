using IdentityServer.Modules.Accounts.Contracts.Recovery;

namespace IdentityServer.Modules.Accounts.Abstractions.Recovery
{
    /// <summary>
    /// Coordinates email confirmation and password recovery for IdentityServer accounts.
    /// </summary>
    public interface IAccountRecoveryService
    {
        /// <summary>Creates and queues a new email-confirmation challenge for a newly registered account.</summary>
        /// <param name="user">The newly registered identity user.</param>
        /// <param name="culture">The preferred email culture, when supplied.</param>
        /// <param name="cancellationToken">The cancellation token for persistence operations.</param>
        /// <returns>Whether the confirmation challenge and protected email dispatch were staged successfully.</returns>
        Task<bool> QueueEmailConfirmation(IdentityUser user, string? culture, CancellationToken cancellationToken);

        /// <summary>Queues a replacement email-confirmation challenge when the account is eligible.</summary>
        /// <param name="email">The account email address, handled without disclosing account existence.</param>
        /// <param name="culture">The preferred email culture, when supplied.</param>
        /// <param name="cancellationToken">The cancellation token for persistence operations.</param>
        Task RequestEmailConfirmation(string email, string? culture, CancellationToken cancellationToken);

        /// <summary>Confirms an email address when the one-time link and six-digit code match.</summary>
        /// <param name="linkToken">The secret token supplied by the confirmation link.</param>
        /// <param name="code">The separate six-digit confirmation code.</param>
        /// <param name="cancellationToken">The cancellation token for Identity and persistence operations.</param>
        /// <returns>The confirmation outcome and lockout state.</returns>
        Task<AccountVerificationResult> ConfirmEmail(
            string linkToken,
            string code,
            CancellationToken cancellationToken);

        /// <summary>Queues a password-recovery challenge without disclosing account existence.</summary>
        /// <param name="email">The account email address, handled without disclosing account existence.</param>
        /// <param name="culture">The preferred email culture, when supplied.</param>
        /// <param name="cancellationToken">The cancellation token for persistence operations.</param>
        Task RequestPasswordRecovery(string email, string? culture, CancellationToken cancellationToken);

        /// <summary>Verifies a recovery link and code and returns a short-lived password-reset grant.</summary>
        /// <param name="linkToken">The secret token supplied by the recovery link.</param>
        /// <param name="code">The separate six-digit recovery code.</param>
        /// <param name="cancellationToken">The cancellation token for persistence operations.</param>
        /// <returns>The verification outcome and a reset grant only when verification succeeds.</returns>
        Task<PasswordRecoveryVerificationResult> VerifyPasswordRecovery(
            string linkToken,
            string code,
            CancellationToken cancellationToken);

        /// <summary>Uses a verified, unconsumed grant to replace the account password.</summary>
        /// <param name="resetGrant">The protected, short-lived grant issued after challenge verification.</param>
        /// <param name="newPassword">The replacement password.</param>
        /// <param name="cancellationToken">The cancellation token for Identity and persistence operations.</param>
        /// <returns>The Identity password result and administrator-required lockout state.</returns>
        Task<PasswordResetResult> ResetPassword(
            string resetGrant,
            string newPassword,
            CancellationToken cancellationToken);
    }
}
