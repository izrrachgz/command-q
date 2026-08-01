using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Modules.Identity.AccountSessions.Queries
{
    public class CheckIfAccountSessionIsLockedByIdQueryValidator : AbstractValidator<CheckIfAccountSessionIsLockedByIdQuery>
    {
        public CheckIfAccountSessionIsLockedByIdQueryValidator(ValidationTextProvider messages)
        {
            RuleFor(r => r.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account_Id))));
        }
    }
}
