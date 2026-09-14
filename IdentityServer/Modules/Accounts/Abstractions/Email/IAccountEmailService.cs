using IdentityServer.Modules.Accounts.Contracts.Challenges;

namespace IdentityServer.Modules.Accounts.Abstractions.Email
{
    /// <summary>
    /// Renders and transactionally stages account confirmation and password recovery emails in the IdentityServer database.
    /// </summary>
    /// <remarks>This abstraction stages protected payloads; it does not deliver email through SMTP.</remarks>
    internal interface IAccountEmailService
    {
        /// <summary>Stages an email containing a confirmation link token and its separate verification code.</summary>
        /// <param name="user">The account awaiting email confirmation.</param>
        /// <param name="challenge">The confirmation challenge to deliver.</param>
        /// <param name="culture">The preferred template culture, when supplied.</param>
        /// <param name="cancellationToken">The cancellation token for the persistence operation.</param>
        Task QueueConfirmation(
            IdentityUser user,
            AccountChallengeDelivery challenge,
            string? culture,
            CancellationToken cancellationToken);

        /// <summary>Stages an email containing a password-recovery link token and its separate verification code.</summary>
        /// <param name="user">The account requesting password recovery.</param>
        /// <param name="challenge">The password-recovery challenge to deliver.</param>
        /// <param name="culture">The preferred template culture, when supplied.</param>
        /// <param name="cancellationToken">The cancellation token for the persistence operation.</param>
        Task QueuePasswordRecovery(
            IdentityUser user,
            AccountChallengeDelivery challenge,
            string? culture,
            CancellationToken cancellationToken);
    }
}
