using FluentValidation;
using Common.Extensions;

namespace Application.Addresses.Commands
{
    public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
    {
        public CreateAddressCommandValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Email is required.")
                .MaximumLength(256)
                .WithMessage("Address's Name cannot exceed 256 characters.");

            RuleFor(x => x.Line1)
                .NotEmpty()
                .WithMessage("Address Line 1 is required.")
                .MaximumLength(256)
                .WithMessage("Address's Line cannot exceed 256 characters.");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("Address City is required.")
                .MaximumLength(256)
                .WithMessage("Address's City cannot exceed 256 characters.");

            RuleFor(x => x.PostalCode)
                .NotEmpty()
                .WithMessage(@"Address Postal Code is required.")
                .MaximumLength(256)
                .WithMessage(@"Address's Postal Code cannot exceed 256 characters.");

            RuleFor(x => x.Country)
                .NotEmpty()
                .WithMessage(@"Address Country is required.")
                .MaximumLength(256)
                .WithMessage(@"Address's Country cannot exceed 256 characters.");
        }
    }
}
