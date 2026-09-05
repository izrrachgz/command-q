namespace Persistence.Abstractions
{
    /// <summary>
    /// Provides infrastructure-level access to provider-specific database session values.
    /// </summary>
    /// <remarks>
    /// Application handlers should use <see cref="IReadOnlyRepository"/> or
    /// <see cref="IWritableRepository"/>. Session context is owned by cross-cutting infrastructure.
    /// </remarks>
    public interface IDatabaseSessionContextRepository
    {
        /// <summary>
        /// Applies database-session values used by provider-specific features such as row-level security.
        /// Providers that do not support session context ignore the values.
        /// </summary>
        /// <param name="values">Session values to apply to the repository connection.</param>
        /// <param name="cancellationToken">Token used while applying the session values.</param>
        Task SetSessionContextAsync(
            IReadOnlyCollection<DatabaseSessionValue> values,
            CancellationToken cancellationToken = default);
    }
}
