using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Modules.Identity.Accounts.Queries
{
    internal sealed class GetAccountByIdQueryValidator : AbstractValidator<GetAccountByIdQuery>
    {
        public GetAccountByIdQueryValidator(ValidationTextProvider messages)
        {
            RuleFor(r => r.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account_Id))));
        }
    }
}
