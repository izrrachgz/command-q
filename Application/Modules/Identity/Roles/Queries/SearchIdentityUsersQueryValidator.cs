using FluentValidation;
using Common.Models.Request;

namespace Application.Modules.Identity.Roles.Queries
{
    internal sealed class SearchIdentityUsersQueryValidator : AbstractValidator<SearchIdentityUsersQuery>
    {
        public SearchIdentityUsersQueryValidator(IValidator<SearchCriteria> searchCriteriaValidator)
        {
            RuleFor(x => x.SearchCriteria)
                .NotNull()
                .SetValidator(searchCriteriaValidator);
        }
    }
}
