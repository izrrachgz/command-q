using Public;
using FluentValidation;
using Common.Models.Request;
using Localization.Providers;

namespace Application.Modules.Configuration.Queries
{
    internal sealed class GetApplicationConfigurationsQueryValidator : AbstractValidator<GetApplicationConfigurationsQuery>
    {
        public GetApplicationConfigurationsQueryValidator(ValidationTextProvider messages, IValidator<SearchCriteria> searchCriteriaValidator)
        {
            RuleFor(x => x.SearchCriteria)
                .NotNull()
                .SetValidator(searchCriteriaValidator);
        }
    }
}
