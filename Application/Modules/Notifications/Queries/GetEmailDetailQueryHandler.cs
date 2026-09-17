using Application.Modules.Notifications.Dtos;
using Cqrs.Abstractions;
using Email.Client.Abstractions;
using Email.Client.Models;

namespace Application.Modules.Notifications.Queries;

internal sealed class GetEmailDetailQueryHandler(IEmailQueueClient emailQueueClient)
    : IQueryHandler<GetEmailDetailQuery, QueuedEmailDetailModel?>
{
    public async Task<QueuedEmailDetailModel?> Handle(GetEmailDetailQuery request, CancellationToken cancellationToken)
    {
        QueuedEmailDetail? detail = await emailQueueClient.GetDetailAsync(request.Id, cancellationToken);
        return detail is null ? null : Map(detail);
    }

    private static QueuedEmailDetailModel Map(QueuedEmailDetail source) => new()
    {
        Id = source.Id,
        Message = Map(source.Message),
        ProfileName = source.ProfileName,
        IdempotencyKey = source.IdempotencyKey,
        AccountReference = source.AccountReference,
        CorrelationId = source.CorrelationId,
        Status = source.Status.ToString(),
        AttemptCount = source.AttemptCount,
        CreatedUtc = source.CreatedUtc,
        ModifiedUtc = source.ModifiedUtc,
        NextAttemptUtc = source.NextAttemptUtc,
        SentUtc = source.SentUtc,
        DeadLetterUtc = source.DeadLetterUtc,
        LastFailureKind = source.LastFailureKind,
        MessageId = source.MessageId
    };

    private static EmailMessageModel Map(EmailMessageDetail source) => new()
    {
        From = source.From is null ? null : Map(source.From),
        To = source.To.Select(Map).ToArray(),
        Cc = source.Cc.Select(Map).ToArray(),
        Bcc = source.Bcc.Select(Map).ToArray(),
        ReplyTo = source.ReplyTo.Select(Map).ToArray(),
        Subject = source.Subject,
        TextBody = source.TextBody,
        HtmlBody = source.HtmlBody,
        Attachments = source.Attachments.Select(Map).ToArray(),
        InlineResources = source.InlineResources.Select(Map).ToArray()
    };

    private static EmailAddressModel Map(EmailAddress source) => new()
    {
        Address = source.Address,
        Name = source.Name
    };

    private static EmailContentModel Map(EmailContentMetadata source) => new()
    {
        FileName = source.FileName,
        ContentType = source.ContentType,
        ContentId = source.ContentId,
        Size = source.Size
    };
}
