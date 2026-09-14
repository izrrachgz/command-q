namespace IdentityServer.Modules.Accounts.Abstractions.Email
{
    /// <summary>
    /// Relays transactionally staged account emails to the external email queue and removes retained account security data.
    /// </summary>
    internal interface IAccountEmailMaintenanceService
    {
        /// <summary>Processes due account-email dispatches, recording success, abandonment, or the next retry time.</summary>
        /// <param name="cancellationToken">The cancellation token for queue and persistence operations.</param>
        Task ProcessPending(CancellationToken cancellationToken);

        /// <summary>Deletes account challenges and security events older than their configured retention periods.</summary>
        /// <param name="cancellationToken">The cancellation token for the cleanup operation.</param>
        Task Cleanup(CancellationToken cancellationToken);
    }
}
