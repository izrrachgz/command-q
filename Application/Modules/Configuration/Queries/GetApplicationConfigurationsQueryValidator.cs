using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Modules.Configuration.Queries
{
    internal sealed class GetApplicationConfigurationsQueryValidator : AbstractValidator<GetApplicationConfigurationsQuery>
    {
        public GetApplicationConfigurationsQueryValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.SearchCriteria.Page)
                .GreaterThan(0)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Be), "Page", "less than 1"));
            RuleFor(x => x.SearchCriteria.Size)
                .InclusiveBetween(1, 100)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Be), "Size", "outside 1-100"));
        }
    }
}
