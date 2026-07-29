using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.AccountSessions.Commands
{
    public class UpdateAccountSessionCommandValidator : AbstractValidator<UpdateAccountSessionCommand>
    {
        public UpdateAccountSessionCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account_Id))));
        }
    }
}
