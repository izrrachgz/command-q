using Public;
using FluentValidation;
using Common.Extensions;
using Localization.Providers;
using Application.Constants;

namespace Application.Contacts.Commands
{
    public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
    {
        public CreateContactCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Name))))
                .MaximumLength(ValidationLimits.MaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), messages.Get(nameof(FieldNames.Name)), ValidationLimits.MaxLength));
        }
    }
}
