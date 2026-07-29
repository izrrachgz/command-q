using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Files.Queries
{
    public class GetAccountFilesQueryValidator : AbstractValidator<GetAccountFilesQuery>
    {
        public GetAccountFilesQueryValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account_Id))));
        }
    }
}
