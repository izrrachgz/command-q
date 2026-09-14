using System.Globalization;
using System.Security.Cryptography;
using IdentityServer.Modules.Accounts.Abstractions.Challenges;
using IdentityServer.Modules.Accounts.Contracts.Challenges;
using System.Text;
using IdentityServer.Modules.Accounts.Configuration;
using IdentityServer.Persistence;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Time.Extensions;

namespace IdentityServer.Modules.Accounts.Services.Challenges
{
    /// <summary>
    /// Persists hashed one-time account challenges and enforces verification and password-reset failure budgets.
    /// </summary>
    /// <remarks>
    /// Link tokens and six-digit codes are generated independently. Only keyed hashes are stored, and verified challenges remain
    /// reusable only until their owning workflow explicitly consumes them.
    /// </remarks>
    internal sealed class AccountChallengeService(
        IdentityServerDbContext database,
        TimeProvider timeProvider,
        IOptions<AccountEmailOptions> options) : IAccountChallengeService
    {
        /// <inheritdoc />
        public async Task<AccountChallengeDelivery?> Create(
            IdentityUser user,
            string purpose,
            TimeSpan lifetime,
            CancellationToken cancellationToken)
        {
            var now = timeProvider.GetCurrentUtcDateTime();
            var recentChallenges = database.AccountChallenges
                .Where(value => value.UserId == user.Id && value.Purpose == purpose);
            var latestCreatedUtc = await recentChallenges
                .MaxAsync(value => (DateTime?)value.CreatedUtc, cancellationToken);

            if (latestCreatedUtc > now.AddSeconds(-options.Value.ResendCooldownSeconds) ||
                await recentChallenges.CountAsync(
                    value => value.CreatedUtc >= now.AddDays(-1),
                    cancellationToken) >= options.Value.DailyDeliveryLimit)
            {
                return null;
            }

            var currentChallenges = await database.AccountChallenges
                .Where(value => value.UserId == user.Id &&
                    value.Purpose == purpose &&
                    value.ConsumedUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var currentChallenge in currentChallenges)
            {
                currentChallenge.ConsumedUtc = now;
                currentChallenge.ConcurrencyStamp = Guid.NewGuid().ToString("N");
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(AccountConstants.ChallengeTokenBytes);
            var linkToken = WebEncoders.Base64UrlEncode(tokenBytes);
            var code = RandomNumberGenerator
                .GetInt32(AccountConstants.VerificationCodeUpperBound)
                .ToString(AccountConstants.VerificationCodeFormat, CultureInfo.InvariantCulture);
            var challenge = new IdentityAccountChallenge
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Purpose = purpose,
                LinkTokenHash = HashToken(tokenBytes),
                CodeHash = HashCode(tokenBytes, code),
                ConcurrencyStamp = Guid.NewGuid().ToString("N"),
                CreatedUtc = now,
                ExpiresUtc = now.Add(lifetime)
            };

            database.AccountChallenges.Add(challenge);
            await database.SaveChangesAsync(cancellationToken);

            return new AccountChallengeDelivery(challenge.Id, linkToken, code, challenge.ExpiresUtc);
        }

        /// <inheritdoc />
        public async Task<AccountChallengeVerificationResult> Verify(
            string linkToken,
            string code,
            string purpose,
            CancellationToken cancellationToken)
        {
            if (!TryDecodeToken(linkToken, out var tokenBytes))
            {
                return AccountChallengeVerificationResult.Invalid();
            }

            var tokenHash = HashToken(tokenBytes);
            var challenge = await database.AccountChallenges
                .Include(value => value.User)
                .SingleOrDefaultAsync(value =>
                    value.LinkTokenHash == tokenHash && value.Purpose == purpose,
                    cancellationToken);

            if (challenge is null ||
                challenge.ConsumedUtc.HasValue ||
                challenge.VerifiedUtc.HasValue ||
                challenge.ExpiresUtc <= timeProvider.GetCurrentUtcDateTime() ||
                IsLocked(challenge.User))
            {
                return AccountChallengeVerificationResult.Invalid(IsLocked(challenge?.User), challenge?.UserId);
            }

            if (!IsValidCodeShape(code))
            {
                var lockedOut = await RegisterVerificationFailure(challenge, challenge.User, cancellationToken);
                return AccountChallengeVerificationResult.Invalid(lockedOut, challenge.UserId);
            }

            var actualHash = Convert.FromHexString(challenge.CodeHash);
            var suppliedHash = Convert.FromHexString(HashCode(tokenBytes, code));

            if (!CryptographicOperations.FixedTimeEquals(actualHash, suppliedHash))
            {
                var lockedOut = await RegisterVerificationFailure(challenge, challenge.User, cancellationToken);
                return AccountChallengeVerificationResult.Invalid(lockedOut, challenge.UserId);
            }

            challenge.VerifiedUtc ??= timeProvider.GetCurrentUtcDateTime();
            challenge.ConcurrencyStamp = Guid.NewGuid().ToString("N");

            try
            {
                await database.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return AccountChallengeVerificationResult.Invalid(userId: challenge.UserId);
            }

            return AccountChallengeVerificationResult.Success(challenge, challenge.User);
        }

        /// <inheritdoc />
        public async Task Consume(IdentityAccountChallenge challenge, CancellationToken cancellationToken)
        {
            challenge.ConsumedUtc = timeProvider.GetCurrentUtcDateTime();
            challenge.ConcurrencyStamp = Guid.NewGuid().ToString("N");

            try
            {
                await database.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await database.Entry(challenge).ReloadAsync(cancellationToken);
            }
        }

        /// <inheritdoc />
        public Task<bool> RegisterVerificationFailure(
            IdentityAccountChallenge challenge,
            IdentityUser user,
            CancellationToken cancellationToken)
        {
            return RegisterFailure(challenge, user, passwordReset: false, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> RegisterPasswordResetFailure(
            IdentityAccountChallenge challenge,
            IdentityUser user,
            CancellationToken cancellationToken)
        {
            return RegisterFailure(challenge, user, passwordReset: true, cancellationToken);
        }

        private async Task<bool> RegisterFailure(
            IdentityAccountChallenge challenge,
            IdentityUser user,
            bool passwordReset,
            CancellationToken cancellationToken)
        {
            for (var retry = 0; retry < AccountConstants.ChallengeConcurrencyRetries; retry++)
            {
                if (passwordReset)
                {
                    challenge.PasswordResetFailedAttempts++;
                }
                else
                {
                    challenge.VerificationFailedAttempts++;
                }

                challenge.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                var attempts = passwordReset
                    ? challenge.PasswordResetFailedAttempts
                    : challenge.VerificationFailedAttempts;
                var maxAttempts = passwordReset
                    ? options.Value.PasswordResetMaxFailedAttempts
                    : options.Value.VerificationMaxFailedAttempts;
                var exhausted = attempts >= maxAttempts;
                var lockedOut = exhausted &&
                    (passwordReset || challenge.Purpose == AccountConstants.ChallengePurposes.PasswordRecovery);

                if (exhausted)
                {
                    challenge.ConsumedUtc = timeProvider.GetCurrentUtcDateTime();
                }

                if (lockedOut)
                {
                    user.RequiresAdminUnlock = true;
                    user.AdminLockoutUtc = timeProvider.GetCurrentUtcDateTime();
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    user.ModifiedUtc = timeProvider.GetCurrentUtcDateTime();
                }

                try
                {
                    await database.SaveChangesAsync(cancellationToken);
                    return lockedOut;
                }
                catch (DbUpdateConcurrencyException)
                {
                    await database.Entry(challenge).ReloadAsync(cancellationToken);
                    await database.Entry(user).ReloadAsync(cancellationToken);

                    if (challenge.ConsumedUtc.HasValue)
                    {
                        return IsLocked(user);
                    }
                }
            }

            return IsLocked(user);
        }

        private bool IsLocked(IdentityUser? user)
        {
            return user?.RequiresAdminUnlock == true ||
                (user?.LockoutEnabled == true && user.LockoutEnd > timeProvider.GetUtcNow());
        }

        private static bool TryDecodeToken(string linkToken, out byte[] tokenBytes)
        {
            try
            {
                tokenBytes = WebEncoders.Base64UrlDecode(linkToken.Trim());
                return tokenBytes.Length == AccountConstants.ChallengeTokenBytes;
            }
            catch (FormatException)
            {
                tokenBytes = [];
                return false;
            }
        }

        private static bool IsValidCodeShape(string code)
        {
            return code.Length == AccountConstants.VerificationCodeLength && code.All(char.IsAsciiDigit);
        }

        private static string HashToken(byte[] tokenBytes)
        {
            return Convert.ToHexString(SHA256.HashData(tokenBytes));
        }

        private static string HashCode(byte[] tokenBytes, string code)
        {
            var codeBytes = Encoding.ASCII.GetBytes(code);
            var combined = new byte[tokenBytes.Length + codeBytes.Length];
            tokenBytes.CopyTo(combined, 0);
            codeBytes.CopyTo(combined, tokenBytes.Length);
            return Convert.ToHexString(SHA256.HashData(combined));
        }
    }
}
