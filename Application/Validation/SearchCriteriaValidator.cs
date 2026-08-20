using Public;
using FluentValidation;
using Application.Constants;
using Common.Models.Request;
using Localization.Providers;

namespace Application.Validation
{
    internal sealed class SearchCriteriaValidator : AbstractValidator<SearchCriteria>
    {
        public SearchCriteriaValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Be), nameof(SearchCriteria.Page), "less than 1"));
            RuleFor(x => x.Size)
                .InclusiveBetween(1, ValidationLimits.MaxSearchPageSize)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Be),
                    nameof(SearchCriteria.Size),
                    $"outside 1-{ValidationLimits.MaxSearchPageSize}"));
            RuleFor(x => x.StartDate)
                .NotNull()
                .When(x => x.EndDate.HasValue)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(SearchCriteria.StartDate)));
            RuleFor(x => x.EndDate)
                .NotNull()
                .When(x => x.StartDate.HasValue)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(SearchCriteria.EndDate)));
            RuleFor(x => x.StartDate)
                .Must(IsUtc)
                .When(x => x.StartDate.HasValue)
                .WithMessage(messages.Get(nameof(ValidationText.Search_Date_Must_Be_Utc), nameof(SearchCriteria.StartDate)));
            RuleFor(x => x.EndDate)
                .Must(IsUtc)
                .When(x => x.EndDate.HasValue)
                .WithMessage(messages.Get(nameof(ValidationText.Search_Date_Must_Be_Utc), nameof(SearchCriteria.EndDate)));
            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Search_Date_Range_Order),
                    nameof(SearchCriteria.EndDate),
                    nameof(SearchCriteria.StartDate)));
        }

        private static bool IsUtc(DateTimeOffset? value)
        {
            return !value.HasValue || value.Value.Offset == TimeSpan.Zero;
        }
    }
}
