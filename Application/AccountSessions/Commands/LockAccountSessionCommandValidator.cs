using Public;
using Common.Extensions;
using FluentValidation;
using Localization.Providers;

namespace Application.AccountSessions.Commands
{
    public class LockAccountSessionCommandValidator : AbstractValidator<LockAccountSessionCommand>
    {
        public LockAccountSessionCommandValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Account Id"));

            RuleFor(x => x.LockTime)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Lock Duration"))
                .LessThanOrEqualTo(TimeSpan.Zero)
                .WithMessage(messages.Get(nameof(ValidationMessages.Duration_Cannot_Be_Zero)));
        }
    }
}
