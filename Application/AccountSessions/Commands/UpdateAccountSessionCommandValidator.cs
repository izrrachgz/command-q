using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.AccountSessions.Commands
{
    public class UpdateAccountSessionCommandValidator : AbstractValidator<UpdateAccountSessionCommand>
    {
        public UpdateAccountSessionCommandValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Account Id"));
        }
    }
}
