using Public;
using FluentValidation;
using System.Globalization;
using Application.Extensions;
using Localization.Providers;

namespace Application.Modules.Identity.Accounts.Preferences.Commands
{
    internal sealed class SaveAccountPreferencesCommandValidator : AbstractValidator<SaveAccountPreferencesCommand>
    {
        public SaveAccountPreferencesCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(SaveAccountPreferencesCommand.AccountId)));
            RuleFor(x => x.Theme)
                .IsInEnum()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Be), nameof(SaveAccountPreferencesCommand.Theme), "invalid"));
            RuleFor(x => x.Culture)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(SaveAccountPreferencesCommand.Culture)))
                .MaximumLength(AccountPreferenceConstants.CultureMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(SaveAccountPreferencesCommand.Culture),
                    AccountPreferenceConstants.CultureMaxLength))
                .Must(IsValidCulture)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Be),
                    nameof(SaveAccountPreferencesCommand.Culture),
                    "invalid"));
            RuleFor(x => x.TimeZone)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(SaveAccountPreferencesCommand.TimeZone)))
                .MaximumLength(AccountPreferenceConstants.TimeZoneMaxLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    nameof(SaveAccountPreferencesCommand.TimeZone),
                    AccountPreferenceConstants.TimeZoneMaxLength))
                .Must(DateTimeUtcExtensions.IsValidTimeZone)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Be),
                    nameof(SaveAccountPreferencesCommand.TimeZone),
                    "invalid"));
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
