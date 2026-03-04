using Public;
using Common.Extensions;
using FluentValidation;
using Localization.Providers;

namespace Application.AccountSessions.Commands
{
    public class UnlockAccountSessionCommandValidator : AbstractValidator<UnlockAccountSessionCommand>
    {
        public UnlockAccountSessionCommandValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Account Id"));
        }
    }
}
