using Api.Logging;

namespace Api.Modules.Notifications;

internal static partial class NotificationEmailsLog
{
    [LoggerMessage(
        EventId = ApiEventIds.EmailAdminSearch,
        Level = LogLevel.Information,
        Message = "Administrator {AdministratorSubject} searched email history. Account filter: {HasAccountFilter}; text filter: {HasTextFilter}; recipient filter: {HasRecipientFilter}; status filter: {HasStatusFilter}; date filter: {HasDateFilter}; matches: {Total}.")]
    public static partial void AdminSearch(
        this ILogger logger,
        string administratorSubject,
        bool hasAccountFilter,
        bool hasTextFilter,
        bool hasRecipientFilter,
        bool hasStatusFilter,
        bool hasDateFilter,
        long total);

    [LoggerMessage(
        EventId = ApiEventIds.EmailAdminDetail,
        Level = LogLevel.Information,
        Message = "Administrator {AdministratorSubject} opened queued email {EmailId}.")]
    public static partial void AdminDetail(this ILogger logger, string administratorSubject, Guid emailId);
}
