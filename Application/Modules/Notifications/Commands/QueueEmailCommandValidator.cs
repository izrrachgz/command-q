using Public;
using FluentValidation;
using Localization.Providers;
using Application.Modules.Notifications.Models;
using Application.Modules.Notifications.Validation;

namespace Application.Modules.Notifications.Commands
{
    public sealed class QueueEmailCommandValidator : AbstractValidator<QueueEmailCommand>
    {
        public QueueEmailCommandValidator(IValidator<ApplicationEmailRequest> emailValidator, ValidationTextProvider messages)
        {
            RuleFor(command => command.Email)
                .NotNull()
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Is_Required),
                    messages.Get(nameof(FieldNames.Email_Message))))
                .SetValidator(emailValidator);
        }
    }
}
