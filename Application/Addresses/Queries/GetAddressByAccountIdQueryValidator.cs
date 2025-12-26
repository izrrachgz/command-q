using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Addresses.Queries
{
    public class GetAddressByAccountIdQueryValidator : AbstractValidator<GetAddressByAccountIdQuery>
    {
        public GetAddressByAccountIdQueryValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(q => q.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), "Account"))
                .GreaterThan(0)
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Cannot_Be), "Account", 0));
        }
    }
}
