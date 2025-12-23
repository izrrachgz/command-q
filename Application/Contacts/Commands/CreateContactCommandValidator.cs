using FluentValidation;
using Common.Extensions;

namespace Application.Contacts.Commands
{
    public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
    {
        public CreateContactCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(@"Contact's Name is required.")
                .MaximumLength(256)
                .WithMessage(@"Name cannot exceed 256 characters.");
        }
    }
}
