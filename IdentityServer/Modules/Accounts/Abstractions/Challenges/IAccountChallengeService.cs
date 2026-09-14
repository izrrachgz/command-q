using IdentityServer.Modules.Accounts.Contracts.Challenges;

namespace IdentityServer.Modules.Accounts.Abstractions.Challenges
{
    /// <summary>
    /// Creates, verifies, consumes, and applies failure budgets to persisted one-time account challenges.
    /// </summary>
    internal interface IAccountChallengeService
    {
        /// <summary>Replaces any active challenge for the same user and purpose with a new challenge.</summary>
        /// <param name="user">The identity user that owns the challenge.</param>
        /// <param name="purpose">The stable purpose that prevents a challenge from being used by another workflow.</param>
        /// <param name="lifetime">The amount of time for which the challenge remains valid.</param>
        /// <param name="cancellationToken">The cancellation token for the persistence operation.</param>
        /// <returns>The separately delivered link token and code, or <see langword="null"/> when creation cannot complete.</returns>
        Task<AccountChallengeDelivery?> Create(
            IdentityUser user,
            string purpose,
            TimeSpan lifetime,
            CancellationToken cancellationToken);

        /// <summary>Verifies a one-time link token and code without consuming the challenge.</summary>
        /// <param name="linkToken">The secret token supplied by the confirmation or recovery link.</param>
        /// <param name="code">The separate six-digit code supplied by the user.</param>
        /// <param name="purpose">The workflow purpose expected for the challenge.</param>
        /// <param name="cancellationToken">The cancellation token for the persistence operation.</param>
        /// <returns>The verification result and verified challenge when successful.</returns>
        Task<AccountChallengeVerificationResult> Verify(
            string linkToken,
            string code,
            string purpose,
            CancellationToken cancellationToken);

        /// <summary>Marks a successfully used challenge as consumed so it cannot be replayed.</summary>
        /// <param name="challenge">The verified challenge to consume.</param>
        /// <param name="cancellationToken">The cancellation token for the persistence operation.</param>
        Task Consume(IdentityAccountChallenge challenge, CancellationToken cancellationToken);

        /// <summary>Records a failed code check and invalidates the challenge after its verification budget is exhausted.</summary>
        /// <param name="challenge">The challenge whose verification failed.</param>
        /// <param name="user">The user that owns the challenge.</param>
        /// <param name="cancellationToken">The cancellation token for the persistence operation.</param>
        /// <returns><see langword="true"/> when the failure caused a lockout; otherwise, <see langword="false"/>.</returns>
        Task<bool> RegisterVerificationFailure(
            IdentityAccountChallenge challenge,
            IdentityUser user,
            CancellationToken cancellationToken);

        /// <summary>Records a failed password submission and permanently locks the account after its reset budget is exhausted.</summary>
        /// <param name="challenge">The verified recovery challenge.</param>
        /// <param name="user">The user whose password reset failed.</param>
        /// <param name="cancellationToken">The cancellation token for the persistence operation.</param>
        /// <returns><see langword="true"/> when the failure caused a lockout; otherwise, <see langword="false"/>.</returns>
        Task<bool> RegisterPasswordResetFailure(
            IdentityAccountChallenge challenge,
            IdentityUser user,
            CancellationToken cancellationToken);
    }
}
