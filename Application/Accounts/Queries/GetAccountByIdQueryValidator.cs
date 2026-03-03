using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Accounts.Queries
{
    public class GetAccountByIdQueryValidator : AbstractValidator<GetAccountByIdQuery>
    {
        public GetAccountByIdQueryValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(r => r.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), "Account Id"));
        }
    }
}
