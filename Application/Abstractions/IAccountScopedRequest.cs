namespace Application.Abstractions
{
    /// <summary>
    /// Identifies an application request whose database work is restricted to one account.
    /// </summary>
    public interface IAccountScopedRequest
    {
        /// <summary>
        /// Account that owns the data accessible while the request is handled.
        /// </summary>
        Guid AccountId { get; }
    }
}
