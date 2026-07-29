using Public;
using FluentValidation;
using Localization.Providers;
using Application.Constants;

namespace Application.Accounts.Queries
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

            RuleFor(r => r.Password)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Password))))
                .MinimumLength(ValidationLimits.MinLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Minimum_Length), messages.Get(nameof(FieldNames.Password)), ValidationLimits.MinLength));
        }
    }
}
