using Public;
using FluentValidation;
using System.Globalization;
using Application.Extensions;
using Localization.Providers;

namespace Application.Modules.Configuration.Commands
{
    internal sealed class SaveApplicationConfigurationCommandValidator : AbstractValidator<SaveApplicationConfigurationCommand>
    {
        public SaveApplicationConfigurationCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.ServerTimeZone)
                .NotEmpty()
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Is_Required),
                    nameof(SaveApplicationConfigurationCommand.ServerTimeZone)))
                .MaximumLength(ApplicationConfigurationConstants.TimeZoneMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(SaveApplicationConfigurationCommand.ServerTimeZone),
                    ApplicationConfigurationConstants.TimeZoneMaxLength))
                .Must(DateTimeUtcExtensions.IsValidTimeZone)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Be),
                    nameof(SaveApplicationConfigurationCommand.ServerTimeZone),
                    "invalid"));
            RuleFor(x => x.ApplicationVersion)
                .MaximumLength(ApplicationConfigurationConstants.VersionMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(SaveApplicationConfigurationCommand.ApplicationVersion),
                    ApplicationConfigurationConstants.VersionMaxLength));
            RuleFor(x => x.Language)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(SaveApplicationConfigurationCommand.Language)))
                .MaximumLength(ApplicationConfigurationConstants.LanguageMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(SaveApplicationConfigurationCommand.Language),
                    ApplicationConfigurationConstants.LanguageMaxLength))
                .Must(IsValidCulture)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Be),
                    nameof(SaveApplicationConfigurationCommand.Language),
                    "invalid"));
            RuleFor(x => x.Environment)
                .NotEmpty()
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Is_Required),
                    nameof(SaveApplicationConfigurationCommand.Environment)))
                .MaximumLength(ApplicationConfigurationConstants.EnvironmentMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(SaveApplicationConfigurationCommand.Environment),
                    ApplicationConfigurationConstants.EnvironmentMaxLength));
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
