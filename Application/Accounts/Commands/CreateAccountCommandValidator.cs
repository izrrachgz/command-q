using Public;
using Common.Extensions;
using FluentValidation;
using Localization.Providers;
using Application.Constants;

namespace Application.Accounts.Commands
{
    public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Email_Address))))
                .MaximumLength(ValidationLimits.MaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), messages.Get(nameof(FieldNames.Email_Address)), ValidationLimits.MaxLength))
                .Must(email => email.IsEmailAddress())
                .WithMessage(messages.Get(nameof(ValidationText.Email_Has_To_Be_Valid)));

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Username))))
                .MaximumLength(ValidationLimits.MaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), messages.Get(nameof(FieldNames.Username)), ValidationLimits.MaxLength));

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Password))))
                .MaximumLength(ValidationLimits.MaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), messages.Get(nameof(FieldNames.Password)), ValidationLimits.MaxLength));
        }
    }
}
