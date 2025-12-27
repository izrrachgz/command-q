using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Accounts.Commands
{
    public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
    {
        public UpdateAccountCommandValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), "Account Id"));

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage(messages.Get(nameof(ValidationMessages.Email_Has_To_Be_Valid), "Email"));
        }
    }
}
