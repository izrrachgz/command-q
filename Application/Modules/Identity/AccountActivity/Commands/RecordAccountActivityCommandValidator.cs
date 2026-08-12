using Public;
using FluentValidation;
using Application.Constants;
using Localization.Providers;

namespace Application.Modules.Identity.AccountActivity.Commands
{
    internal sealed class RecordAccountActivityCommandValidator : AbstractValidator<RecordAccountActivityCommand>
    {
        public RecordAccountActivityCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(RecordAccountActivityCommand.AccountId)));

            RuleFor(x => x.Module)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(RecordAccountActivityCommand.Module)))
                .MaximumLength(AccountActivityConstants.ModuleMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(RecordAccountActivityCommand.Module), AccountActivityConstants.ModuleMaxLength));

            RuleFor(x => x.Action)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(RecordAccountActivityCommand.Action)))
                .MaximumLength(AccountActivityConstants.ActionMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(RecordAccountActivityCommand.Action), AccountActivityConstants.ActionMaxLength));

            RuleFor(x => x.EntityType)
                .MaximumLength(AccountActivityConstants.EntityTypeMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(RecordAccountActivityCommand.EntityType), AccountActivityConstants.EntityTypeMaxLength));

            RuleFor(x => x.EntityId)
                .MaximumLength(AccountActivityConstants.EntityIdMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(RecordAccountActivityCommand.EntityId), AccountActivityConstants.EntityIdMaxLength));

            RuleFor(x => x.Message)
                .MaximumLength(AccountActivityConstants.MessageMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(RecordAccountActivityCommand.Message), AccountActivityConstants.MessageMaxLength));

            RuleFor(x => x.MetadataJson)
                .MaximumLength(AccountActivityConstants.MetadataJsonMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(RecordAccountActivityCommand.MetadataJson), AccountActivityConstants.MetadataJsonMaxLength));

            RuleFor(x => x.IpAddress)
                .MaximumLength(AccountActivityConstants.IpAddressMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(RecordAccountActivityCommand.IpAddress), AccountActivityConstants.IpAddressMaxLength));

            RuleFor(x => x.UserAgent)
                .MaximumLength(AccountActivityConstants.UserAgentMaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), nameof(RecordAccountActivityCommand.UserAgent), AccountActivityConstants.UserAgentMaxLength));
        }
    }
}
