using Common.Cqrs;
using Application.Modules.Notifications.Models;
using Application.Modules.Notifications.Services;

namespace Application.Modules.Notifications.Commands
{
    internal sealed class QueueEmailCommandHandler(IApplicationEmailService emailService) : ICommandHandler<QueueEmailCommand, QueuedEmailResult>
    {
        public async Task<QueuedEmailResult> Handle(QueueEmailCommand request, CancellationToken cancellationToken)
        {
            return await emailService.QueueAsync(request.Email, cancellationToken);
        }
    }
}
