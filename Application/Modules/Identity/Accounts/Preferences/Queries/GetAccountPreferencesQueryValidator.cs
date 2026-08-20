using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Modules.Identity.Accounts.Preferences.Queries
{
    internal sealed class GetAccountPreferencesQueryValidator : AbstractValidator<GetAccountPreferencesQuery>
    {
        public GetAccountPreferencesQueryValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(GetAccountPreferencesQuery.AccountId)));
        }
    }
}
