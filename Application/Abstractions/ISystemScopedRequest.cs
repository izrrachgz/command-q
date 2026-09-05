namespace Application.Abstractions
{
    /// <summary>
    /// Identifies a trusted application request that requires cross-account database access.
    /// </summary>
    /// <remarks>
    /// Use this contract only for internal workflows such as resolving or provisioning an account.
    /// </remarks>
    public interface ISystemScopedRequest
    {
    }
}
