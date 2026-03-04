using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Contacts.Queries
{
    public class GetContactByAccountIdQueryValidator : AbstractValidator<GetContactByAccountIdQuery>
    {
        public GetContactByAccountIdQueryValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(q => q.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Account Id"));
        }
    }
}
