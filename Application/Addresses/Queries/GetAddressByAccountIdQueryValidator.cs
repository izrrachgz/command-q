using Public;
using FluentValidation;
using Localization.Abstractions;

namespace Application.Addresses.Queries
{
    public class GetAddressByAccountIdQueryValidator : AbstractValidator<GetAddressByAccountIdQuery>
    {
        public GetAddressByAccountIdQueryValidator(ITextProvider messages)
        {
            RuleFor(q => q.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Is_Required), "Account"))
                .GreaterThan(0)
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Cannot_Be), "Account", 0));
        }
    }
}
