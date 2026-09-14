using System.Text.Json;
using IdentityServer.Modules.Accounts.Abstractions.Email;
using IdentityServer.Modules.Accounts.Contracts.Challenges;
using IdentityServer.Modules.Accounts.Contracts.Email;
using IdentityServer.Modules.Accounts.Payloads;
using IdentityServer.Modules.Accounts.Configuration;
using IdentityServer.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Time.Extensions;

namespace IdentityServer.Modules.Accounts.Services.Email
{
    /// <summary>
    /// Renders account emails and stages their protected dispatch payloads in the current IdentityServer transaction.
    /// </summary>
    /// <remarks>
    /// This service does not call SMTP or the Email API. The maintenance worker relays committed dispatches asynchronously.
    /// </remarks>
    internal sealed class AccountEmailService : IAccountEmailService
    {
        private readonly IdentityServerDbContext database;
        private readonly IAccountEmailTemplateRenderer renderer;
        private readonly AccountEmailOptions options;
        private readonly TimeProvider timeProvider;
        private readonly IDataProtector protector;

        public AccountEmailService(
            IdentityServerDbContext database,
            IAccountEmailTemplateRenderer renderer,
            IOptions<AccountEmailOptions> options,
            TimeProvider timeProvider,
            IDataProtectionProvider dataProtectionProvider)
        {
            this.database = database;
            this.renderer = renderer;
            this.options = options.Value;
            this.timeProvider = timeProvider;
            protector = dataProtectionProvider.CreateProtector(AccountEmailDispatchProtection.Purpose);
        }

        /// <inheritdoc />
        public Task QueueConfirmation(
            IdentityUser user,
            AccountChallengeDelivery challenge,
            string? culture,
            CancellationToken cancellationToken)
        {
            var actionUrl = BuildActionUrl(AccountConstants.FrontendRoutes.ConfirmEmail, challenge.LinkToken);
            var content = renderer.RenderConfirmation(
                user.UserName ?? string.Empty,
                challenge.Code,
                actionUrl,
                challenge.ExpiresUtc,
                culture);

            return Enqueue(user, challenge, AccountConstants.ChallengePurposes.EmailConfirmation, content, cancellationToken);
        }

        /// <inheritdoc />
        public Task QueuePasswordRecovery(
            IdentityUser user,
            AccountChallengeDelivery challenge,
            string? culture,
            CancellationToken cancellationToken)
        {
            var actionUrl = BuildActionUrl(AccountConstants.FrontendRoutes.PasswordRecovery, challenge.LinkToken);
            var content = renderer.RenderPasswordRecovery(
                user.UserName ?? string.Empty,
                challenge.Code,
                actionUrl,
                challenge.ExpiresUtc,
                culture);

            return Enqueue(user, challenge, AccountConstants.ChallengePurposes.PasswordRecovery, content, cancellationToken);
        }

        private async Task Enqueue(
            IdentityUser user,
            AccountChallengeDelivery challenge,
            string purpose,
            AccountEmailContent content,
            CancellationToken cancellationToken)
        {
            var payload = new AccountEmailDispatchPayload(
                user.Email ?? string.Empty,
                user.UserName ?? string.Empty,
                content.Subject,
                content.HtmlBody,
                content.TextBody,
                NullIfWhiteSpace(options.ProfileName),
                $"identity:{purpose}:{challenge.ChallengeId:N}",
                user.Id.ToString("N"));
            var now = timeProvider.GetCurrentUtcDateTime();

            database.AccountEmailDispatches.Add(new IdentityAccountEmailDispatch
            {
                Id = Guid.NewGuid(),
                ChallengeId = challenge.ChallengeId,
                ProtectedPayload = protector.Protect(JsonSerializer.Serialize(payload)),
                ConcurrencyStamp = Guid.NewGuid().ToString("N"),
                CreatedUtc = now,
                NextAttemptUtc = now
            });
            await database.SaveChangesAsync(cancellationToken);
        }

        private string BuildActionUrl(string path, string linkToken)
        {
            var baseUrl = options.ApplicationBaseUrl.TrimEnd('/') + "/";
            var uri = new Uri(new Uri(baseUrl, UriKind.Absolute), path.TrimStart('/'));
            var fragment = $"{AccountConstants.LinkTokenFragmentParameter}={Uri.EscapeDataString(linkToken)}";
            return new UriBuilder(uri) { Fragment = fragment }.Uri.ToString();
        }

        private static string? NullIfWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
