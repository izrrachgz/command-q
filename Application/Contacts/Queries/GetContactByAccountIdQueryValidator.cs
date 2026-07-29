using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Contacts.Queries
{
    public class GetContactByAccountIdQueryValidator : AbstractValidator<GetContactByAccountIdQuery>
    {
        public GetContactByAccountIdQueryValidator(ValidationTextProvider messages)
        {
            RuleFor(q => q.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account_Id))));
        }
    }
}
