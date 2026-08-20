using Public;
using FluentValidation;
using Common.Models.Request;
using Localization.Providers;

namespace Application.Modules.Identity.AccountActivity.Queries
{
    internal sealed class GetAccountActivityQueryValidator : AbstractValidator<GetAccountActivityQuery>
    {
        public GetAccountActivityQueryValidator(ValidationTextProvider messages, IValidator<SearchCriteria> searchCriteriaValidator)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(GetAccountActivityQuery.AccountId)));

            RuleFor(x => x.Module)
                .MaximumLength(AccountActivityConstants.ModuleMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(GetAccountActivityQuery.Module), AccountActivityConstants.ModuleMaxLength));

            RuleFor(x => x.Action)
                .MaximumLength(AccountActivityConstants.ActionMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(GetAccountActivityQuery.Action), AccountActivityConstants.ActionMaxLength));

            RuleFor(x => x.SearchCriteria)
                .NotNull()
                .SetValidator(searchCriteriaValidator);
        }
    }
}
