using Public;
using Common.Extensions;
using FluentValidation;
using Localization.Providers;

namespace Application.AccountSessions.Commands
{
    public class UnlockAccountSessionCommandValidator : AbstractValidator<UnlockAccountSessionCommand>
    {
        public UnlockAccountSessionCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account_Id))));
        }
    }
}
