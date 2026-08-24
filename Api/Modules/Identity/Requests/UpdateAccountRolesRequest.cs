namespace Api.Modules.Identity.Requests
{
    /// <summary>
    /// Describes the complete role set to assign to an identity account.
    /// </summary>
    public sealed record UpdateAccountRolesRequest
    {
        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
