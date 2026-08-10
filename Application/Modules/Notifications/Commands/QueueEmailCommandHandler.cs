using Common.Cqrs;
using Application.Modules.Notifications.Models;
using Application.Modules.Notifications.Services;

namespace Application.Modules.Notifications.Commands
{
    public sealed class QueueEmailCommandHandler(IApplicationEmailService emailService)
        : ICommandHandler<QueueEmailCommand, QueuedEmailResult>
    {
        public Task<QueuedEmailResult> Handle(QueueEmailCommand request, CancellationToken cancellationToken)
        {
            return emailService.QueueAsync(request.Email, cancellationToken);
        }
    }
}
