using Time.Extensions;
using IdentityServer.Persistence;
using IdentityServer.Modules.Accounts.Logging;

namespace IdentityServer.Modules.Accounts
{
    internal sealed class IdentitySecurityEventRecorder(
        IdentityServerDbContext database,
        IHttpContextAccessor httpContextAccessor,
        TimeProvider timeProvider,
        ILogger<IdentitySecurityEventRecorder> logger) : IIdentitySecurityEventRecorder
    {
        public async Task Record(
            string action,
            Guid? userId = null,
            string? clientId = null,
            string? detailsJson = null,
            CancellationToken cancellationToken = default)
        {
            var request = httpContextAccessor.HttpContext?.Request;
            var securityEvent = new IdentitySecurityEvent
            {
                UserId = userId,
                Action = action,
                ClientId = clientId,
                IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = request?.Headers.UserAgent.ToString(),
                DetailsJson = detailsJson,
                OccurredUtc = timeProvider.GetCurrentUtcDateTime()
            };

            try
            {
                database.SecurityEvents.Add(securityEvent);
                await database.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.SecurityEventRecordFailed(exception, action, userId);
            }
        }
    }
}
