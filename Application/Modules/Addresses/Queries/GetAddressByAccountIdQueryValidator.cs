using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Modules.Addresses.Queries
{
    internal sealed class GetAddressByAccountIdQueryValidator : AbstractValidator<GetAddressByAccountIdQuery>
    {
        public GetAddressByAccountIdQueryValidator(ValidationTextProvider messages)
        {
            RuleFor(q => q.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account))));
        }
    }
}
