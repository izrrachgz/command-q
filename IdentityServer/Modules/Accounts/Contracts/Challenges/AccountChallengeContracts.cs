namespace IdentityServer.Modules.Accounts.Contracts.Challenges
{
    /// <summary>Contains the one-time secrets that must be delivered separately from their persisted hashes.</summary>
    /// <param name="ChallengeId">The persisted challenge identifier.</param>
    /// <param name="LinkToken">The random secret placed in the application URL fragment.</param>
    /// <param name="Code">The separate six-digit code displayed in the email.</param>
    /// <param name="ExpiresUtc">The UTC time at which the challenge stops being valid.</param>
    internal sealed record AccountChallengeDelivery(Guid ChallengeId, string LinkToken, string Code, DateTime ExpiresUtc);

    /// <summary>Pairs a successfully verified challenge with its owning identity user.</summary>
    /// <param name="Challenge">The verified persisted challenge.</param>
    /// <param name="User">The user that owns the challenge.</param>
    internal sealed record VerifiedAccountChallenge(IdentityAccountChallenge Challenge, IdentityUser User);

    /// <summary>Describes an internal challenge-verification attempt and carries verified state only on success.</summary>
    /// <param name="Succeeded">Whether both one-time secrets and the expected purpose matched.</param>
    /// <param name="LockedOut">Whether the account or challenge is locked after the attempt.</param>
    /// <param name="UserId">The owning user identifier when it could be determined safely.</param>
    /// <param name="VerifiedChallenge">The verified challenge and user when verification succeeded.</param>
    internal sealed record AccountChallengeVerificationResult(
        bool Succeeded,
        bool LockedOut,
        Guid? UserId,
        VerifiedAccountChallenge? VerifiedChallenge)
    {
        /// <summary>Creates an unsuccessful verification result.</summary>
        public static AccountChallengeVerificationResult Invalid(bool lockedOut = false, Guid? userId = null)
        {
            return new(false, lockedOut, userId, null);
        }

        /// <summary>Creates a successful verification result containing the challenge and its owner.</summary>
        public static AccountChallengeVerificationResult Success(IdentityAccountChallenge challenge, IdentityUser user)
        {
            return new(true, false, user.Id, new VerifiedAccountChallenge(challenge, user));
        }
    }
}
