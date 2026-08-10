using Common.Cqrs;
using Application.Modules.Notifications.Models;

namespace Application.Modules.Notifications.Commands
{
    public sealed record QueueEmailCommand : ICommand<QueuedEmailResult>
    {
        public required ApplicationEmailRequest Email { get; init; }
    }
}
