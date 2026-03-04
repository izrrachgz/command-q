using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.AccountSessions.Queries
{
    public class CheckIdAccountSessionIsLockedByIdQueryValidator : AbstractValidator<CheckIfAccountSessionIsLockedByIdQuery>
    {
        public CheckIdAccountSessionIsLockedByIdQueryValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(r => r.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Account Id"));
        }
    }
}
