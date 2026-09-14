using Common.Models.Response;
using Cqrs.Abstractions;

namespace Application.Modules.Identity.Roles.Commands
{
    /// <summary>
    /// Requests removal of an administrator-required identity lock.
    /// </summary>
    public sealed record UnlockIdentityUserCommand : ICommand<BasicResponse>
    {
        /// <summary>
        /// Gets the identity user identifier to unlock.
        /// </summary>
        public required Guid UserId { get; init; }
    }
}
