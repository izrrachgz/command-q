using Email.Client;
using Email.Client.Models;
using FluentValidation;
using Localization.Providers;
using Public;

namespace Application.Modules.Notifications.Queries;

internal sealed class SearchEmailsQueryValidator : AbstractValidator<SearchEmailsQuery>
{
    public SearchEmailsQueryValidator(ValidationTextProvider messages)
    {
        RuleFor(query => query.AccountReference).MaximumLength(EmailQueueConstants.MaxReferenceLength);
        RuleFor(query => query.SearchTerm).MaximumLength(256);
        RuleFor(query => query.Recipient).MaximumLength(256);
        RuleFor(query => query.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) ||
                Enum.TryParse<EmailDeliveryStatus>(status, ignoreCase: true, out EmailDeliveryStatus parsed) &&
                parsed != EmailDeliveryStatus.Unspecified)
            .WithMessage(messages.Get(
                nameof(ValidationText.Field_Cannot_Be),
                nameof(SearchEmailsQuery.Status),
                "invalid"));
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Be), nameof(SearchEmailsQuery.Page), "less than 1"));
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, EmailQueueConstants.MaximumPageSize)
            .WithMessage(messages.Get(
                nameof(ValidationText.Field_Cannot_Be),
                nameof(SearchEmailsQuery.PageSize),
                $"outside 1-{EmailQueueConstants.MaximumPageSize}"));
        RuleFor(query => query.StartUtc)
            .Must(value => !value.HasValue || value.Value.Kind == DateTimeKind.Utc)
            .WithMessage(messages.Get(nameof(ValidationText.Search_Date_Must_Be_Utc), nameof(SearchEmailsQuery.StartUtc)));
        RuleFor(query => query.EndUtc)
            .Must(value => !value.HasValue || value.Value.Kind == DateTimeKind.Utc)
            .WithMessage(messages.Get(nameof(ValidationText.Search_Date_Must_Be_Utc), nameof(SearchEmailsQuery.EndUtc)));
        RuleFor(query => query.EndUtc)
            .GreaterThanOrEqualTo(query => query.StartUtc)
            .When(query => query.StartUtc.HasValue && query.EndUtc.HasValue)
            .WithMessage(messages.Get(
                nameof(ValidationText.Search_Date_Range_Order),
                nameof(SearchEmailsQuery.EndUtc),
                nameof(SearchEmailsQuery.StartUtc)));
    }
}
