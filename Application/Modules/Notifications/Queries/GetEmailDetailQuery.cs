using Application.Abstractions;
using Application.Modules.Notifications.Dtos;
using Cqrs.Abstractions;

namespace Application.Modules.Notifications.Queries;

public sealed record GetEmailDetailQuery(Guid Id) : IQuery<QueuedEmailDetailModel?>, ISystemScopedRequest;
