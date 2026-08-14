using Email.Constants;
using Microsoft.Extensions.Options;

namespace Email.Configuration
{
    internal sealed class EmailOptionsValidator : IValidateOptions<EmailOptions>
    {
        public ValidateOptionsResult Validate(string? name, EmailOptions options)
        {
            if (!options.Enabled)
            {
                return ValidateOptionsResult.Success;
            }

            var errors = new List<string>();
            string defaultProfile = options.DefaultProfile.Trim();

            if (string.IsNullOrWhiteSpace(options.DefaultProfile))
            {
                errors.Add("Email:DefaultProfile is required.");
            }
            else if (!string.Equals(options.DefaultProfile, defaultProfile, StringComparison.Ordinal))
            {
                errors.Add("Email:DefaultProfile cannot start or end with whitespace.");
            }
            else if (defaultProfile.Length > EmailConstants.MaxProfileNameLength)
            {
                errors.Add($"Email:DefaultProfile cannot exceed {EmailConstants.MaxProfileNameLength} characters.");
            }

            if (options.Profiles.Count == 0)
            {
                EmailProfileOptionsValidator.Validate(options, "Email", errors);
            }
            else
            {
                if (!options.Profiles.Keys.Any(profileName =>
                    string.Equals(profileName, defaultProfile, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add($"Email:Profiles must contain the configured default profile '{defaultProfile}'.");
                }

                foreach ((string profileName, EmailProfileOptions profile) in options.Profiles)
                {
                    if (string.IsNullOrWhiteSpace(profileName))
                    {
                        errors.Add("Email:Profiles cannot contain an empty profile name.");
                        continue;
                    }

                    if (!string.Equals(profileName, profileName.Trim(), StringComparison.Ordinal))
                    {
                        errors.Add($"Email:Profiles:{profileName} cannot start or end with whitespace.");
                        continue;
                    }

                    if (profileName.Length > EmailConstants.MaxProfileNameLength)
                    {
                        errors.Add($"Email:Profiles:{profileName} cannot exceed {EmailConstants.MaxProfileNameLength} characters.");
                        continue;
                    }

                    EmailProfileOptionsValidator.Validate(profile, $"Email:Profiles:{profileName}", errors);
                }
            }

            return errors.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(errors);
        }
    }
}
