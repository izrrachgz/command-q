using Public;
using FluentValidation;
using Common.Extensions;
using Localization.Providers;

namespace Application.Modules.Identity.AccountSessions.Commands
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
