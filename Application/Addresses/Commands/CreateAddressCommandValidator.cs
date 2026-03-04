using Public;
using FluentValidation;
using Common.Extensions;
using Localization.Providers;

namespace Application.Addresses.Commands
{
    public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
    {
        private const int MAX_LENGTH = 256;

        public CreateAddressCommandValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Account"));

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Name"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Cannot_Exceed), @"Name", MAX_LENGTH));

            RuleFor(x => x.Line1)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Address Line"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Cannot_Exceed), @"Address Line", MAX_LENGTH));

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"City"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Cannot_Exceed), @"City", MAX_LENGTH));

            RuleFor(x => x.PostalCode)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Postal Code"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Cannot_Exceed), @"Postal Code", MAX_LENGTH));

            RuleFor(x => x.Country)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), "Country"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Cannot_Exceed), "Country", MAX_LENGTH));
        }
    }
}
