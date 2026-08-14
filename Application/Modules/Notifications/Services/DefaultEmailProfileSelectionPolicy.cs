using FluentValidation;
using Email.Outbox.Constants;

namespace Application.Modules.Notifications.Services
{
    internal sealed class DefaultEmailProfileSelectionPolicy : IEmailProfileSelectionPolicy
    {
        public void Validate(string? profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName) || string.Equals(profileName, EmailOutboxConstants.DefaultProfileName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new ValidationException($"The application workflow is not authorized to select the email profile '{profileName}'.");
        }
    }
}
