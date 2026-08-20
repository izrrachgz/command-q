using Public;
using FluentValidation;
using Common.Models.Request;
using Localization.Providers;

namespace Application.Modules.Files.Queries
{
    internal sealed class GetAccountFilesQueryValidator : AbstractValidator<GetAccountFilesQuery>
    {
        public GetAccountFilesQueryValidator(ValidationTextProvider messages, IValidator<SearchCriteria> searchCriteriaValidator)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account_Id))));
            RuleFor(x => x.SearchCriteria)
                .NotNull()
                .SetValidator(searchCriteriaValidator);
        }
    }
}
