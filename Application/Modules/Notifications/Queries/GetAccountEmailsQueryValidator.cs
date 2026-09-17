using Email.Client;
using FluentValidation;
using Localization.Providers;
using Public;

namespace Application.Modules.Notifications.Queries;

internal sealed class GetAccountEmailsQueryValidator : AbstractValidator<GetAccountEmailsQuery>
{
    public GetAccountEmailsQueryValidator(ValidationTextProvider messages)
    {
        RuleFor(query => query.AccountId)
            .NotEmpty()
            .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(GetAccountEmailsQuery.AccountId)));
        RuleFor(query => query.AccountReference)
            .NotEmpty()
            .MaximumLength(EmailQueueConstants.MaxReferenceLength);
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Be), nameof(GetAccountEmailsQuery.Page), "less than 1"));
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, EmailQueueConstants.MaximumPageSize)
            .WithMessage(messages.Get(
                nameof(ValidationText.Field_Cannot_Be),
                nameof(GetAccountEmailsQuery.PageSize),
                $"outside 1-{EmailQueueConstants.MaximumPageSize}"));
    }
}
