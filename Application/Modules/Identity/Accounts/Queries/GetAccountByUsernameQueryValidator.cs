using Public;
using FluentValidation;
using Application.Constants;
using Localization.Providers;

namespace Application.Modules.Identity.Accounts.Queries
{
    public class GetAccountByUsernameQueryValidator : AbstractValidator<GetAccountByUsernameQuery>
    {
        public GetAccountByUsernameQueryValidator(ValidationTextProvider messages)
        {
            RuleFor(r => r.Username)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Username))))
                .MinimumLength(ValidationLimits.MinLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Minimum_Length), messages.Get(nameof(FieldNames.Username)), ValidationLimits.MinLength));
        }
    }
}
