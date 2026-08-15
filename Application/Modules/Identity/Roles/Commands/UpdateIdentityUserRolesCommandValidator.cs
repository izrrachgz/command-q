using Public;
using FluentValidation;
using Application.Constants;
using Localization.Providers;

namespace Application.Modules.Identity.Roles.Commands
{
    internal sealed class UpdateIdentityUserRolesCommandValidator : AbstractValidator<UpdateIdentityUserRolesCommand>
    {
        public UpdateIdentityUserRolesCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(command => command.UserId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(UpdateIdentityUserRolesCommand.UserId)));

            RuleFor(command => command.Roles)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(UpdateIdentityUserRolesCommand.Roles)));

            RuleForEach(command => command.Roles)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(UpdateIdentityUserRolesCommand.Roles)))
                .MaximumLength(ValidationLimits.MaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(UpdateIdentityUserRolesCommand.Roles), ValidationLimits.MaxLength));
        }
    }
}
