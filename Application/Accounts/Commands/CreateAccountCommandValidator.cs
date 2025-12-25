using Public;
using FluentValidation;
using Common.Extensions;
using Localization.Abstractions;

namespace Application.Accounts.Commands
{
    public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        private const int MAX_LENGTH = 256;

        public CreateAccountCommandValidator(ITextProvider messages)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Is_Required), "Email Address"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Cannot_Exceed), "Email Address", MAX_LENGTH))
                .Must(email => email.IsEmailAddress())
                .WithMessage(messages.Get(nameof(Messages.Validation_Email_Has_To_Be_Valid)));

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Is_Required), "Username"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Cannot_Exceed), "Username", MAX_LENGTH));

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Is_Required), "Password"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Cannot_Exceed), "Password", MAX_LENGTH));
        }
    }
}
