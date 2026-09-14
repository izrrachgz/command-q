using FluentValidation;
using Localization.Providers;
using Public;

namespace Application.Modules.Identity.Roles.Commands
{
    internal sealed class UnlockIdentityUserCommandValidator : AbstractValidator<UnlockIdentityUserCommand>
    {
        public UnlockIdentityUserCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(command => command.UserId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(UnlockIdentityUserCommand.UserId)));
        }
    }
}
