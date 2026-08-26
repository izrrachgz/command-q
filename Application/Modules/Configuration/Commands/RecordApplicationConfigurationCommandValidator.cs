using Public;
using FluentValidation;
using System.Globalization;
using Time.Extensions;
using Localization.Providers;

namespace Application.Modules.Configuration.Commands
{
    internal sealed class RecordApplicationConfigurationCommandValidator : AbstractValidator<RecordApplicationConfigurationCommand>
    {
        public RecordApplicationConfigurationCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.ServerTimeZone)
                .NotEmpty()
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Is_Required),
                    nameof(RecordApplicationConfigurationCommand.ServerTimeZone)))
                .MaximumLength(ApplicationConfigurationConstants.TimeZoneMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(RecordApplicationConfigurationCommand.ServerTimeZone),
                    ApplicationConfigurationConstants.TimeZoneMaxLength))
                .Must(DateTimeUtcExtensions.IsValidTimeZone)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Be),
                    nameof(RecordApplicationConfigurationCommand.ServerTimeZone),
                    "invalid"));
            RuleFor(x => x.ApplicationVersion)
                .NotEmpty()
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Is_Required),
                    nameof(RecordApplicationConfigurationCommand.ApplicationVersion)))
                .MaximumLength(ApplicationConfigurationConstants.VersionMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(RecordApplicationConfigurationCommand.ApplicationVersion),
                    ApplicationConfigurationConstants.VersionMaxLength));
            RuleFor(x => x.Language)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(RecordApplicationConfigurationCommand.Language)))
                .MaximumLength(ApplicationConfigurationConstants.LanguageMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(RecordApplicationConfigurationCommand.Language),
                    ApplicationConfigurationConstants.LanguageMaxLength))
                .Must(IsValidCulture)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Be),
                    nameof(RecordApplicationConfigurationCommand.Language),
                    "invalid"));
            RuleFor(x => x.Environment)
                .NotEmpty()
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Is_Required),
                    nameof(RecordApplicationConfigurationCommand.Environment)))
                .MaximumLength(ApplicationConfigurationConstants.EnvironmentMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(RecordApplicationConfigurationCommand.Environment),
                    ApplicationConfigurationConstants.EnvironmentMaxLength));
            RuleFor(x => x.InstanceName)
                .NotEmpty()
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Is_Required),
                    nameof(RecordApplicationConfigurationCommand.InstanceName)))
                .MaximumLength(ApplicationConfigurationConstants.InstanceNameMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(RecordApplicationConfigurationCommand.InstanceName),
                    ApplicationConfigurationConstants.InstanceNameMaxLength));
        }

        private static bool IsValidCulture(string culture)
        {
            try
            {
                _ = CultureInfo.GetCultureInfo(culture);
                return true;
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }
    }
}
