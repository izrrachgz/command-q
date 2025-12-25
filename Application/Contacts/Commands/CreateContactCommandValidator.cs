using Public;
using FluentValidation;
using Common.Extensions;
using Localization.Abstractions;

namespace Application.Contacts.Commands
{
    public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
    {
        private const int MAX_LENGTH = 256;

        public CreateContactCommandValidator(ITextProvider messages)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Is_Required), "Name"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Cannot_Exceed), "Name", MAX_LENGTH));
        }
    }
}
