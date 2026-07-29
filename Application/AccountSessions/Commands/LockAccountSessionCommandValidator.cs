using Public;
using Common.Extensions;
using FluentValidation;
using Localization.Providers;

namespace Application.AccountSessions.Commands
{
    public class LockAccountSessionCommandValidator : AbstractValidator<LockAccountSessionCommand>
    {
        public LockAccountSessionCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account_Id))));

            RuleFor(x => x.LockTime)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Lock_Duration))))
                .LessThanOrEqualTo(TimeSpan.Zero)
                .WithMessage(messages.Get(nameof(ValidationText.Duration_Cannot_Be_Zero)));
        }
    }
}
