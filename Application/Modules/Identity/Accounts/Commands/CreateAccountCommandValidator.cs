using Public;
using FluentValidation;
using Common.Extensions;
using Application.Constants;
using Localization.Providers;

namespace Application.Modules.Identity.Accounts.Commands
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

            RuleFor(x => x.IdentityIssuer)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(CreateAccountCommand.IdentityIssuer)))
                .MaximumLength(AccountIdentityConstants.IssuerMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(CreateAccountCommand.IdentityIssuer), AccountIdentityConstants.IssuerMaxLength));

            RuleFor(x => x.IdentitySubject)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(CreateAccountCommand.IdentitySubject)))
                .MaximumLength(AccountIdentityConstants.SubjectMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(CreateAccountCommand.IdentitySubject), AccountIdentityConstants.SubjectMaxLength));
        }
    }
}
