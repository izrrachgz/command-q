using MimeKit;
using Email.Constants;

namespace Email.Configuration
{
    /// <summary>
    /// Validates the transport and delivery settings owned by one email profile.
    /// </summary>
    internal static class EmailProfileOptionsValidator
    {
        /// <summary>
        /// Adds validation failures for a configured email profile.
        /// </summary>
        /// <param name="profile">The profile to validate.</param>
        /// <param name="path">The configuration path used in failure messages.</param>
        /// <param name="errors">The collection that receives validation failures.</param>
        public static void Validate(EmailProfileOptions profile, string path, ICollection<string> errors)
        {
            if (!profile.Enabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(profile.Smtp.Host))
            {
                errors.Add($"{path}:Smtp:Host is required.");
            }

            if (profile.Smtp.Port is < 1 or > 65535)
            {
                errors.Add($"{path}:Smtp:Port must be between 1 and 65535.");
            }

            if (!Enum.IsDefined(profile.Smtp.Security))
            {
                errors.Add($"{path}:Smtp:Security is invalid.");
            }

            if (profile.Smtp.TimeoutMilliseconds <= 0)
            {
                errors.Add($"{path}:Smtp:TimeoutMilliseconds must be greater than zero.");
            }

            if (!Enum.IsDefined(profile.Authentication.Type))
            {
                errors.Add($"{path}:Authentication:Type is invalid.");
            }
            else
            {
                ValidateAuthentication(profile.Authentication, path, errors);

                if (profile.Authentication.Type != EmailAuthenticationType.Anonymous &&
                    profile.Smtp.Security is not EmailSecurityMode.StartTls and not EmailSecurityMode.SslOnConnect)
                {
                    errors.Add($"Authenticated delivery requires {path}:Smtp:Security to be StartTls or SslOnConnect.");
                }
            }

            if (!MailboxAddress.TryParse(profile.Sender.Address, out _))
            {
                errors.Add($"{path}:Sender:Address must be a valid mailbox address.");
            }

            ValidateDeliveryPolicy(profile.DeliveryPolicy, path, errors);
        }

        private static void ValidateDeliveryPolicy(EmailDeliveryPolicyOptions policy, string path, ICollection<string> errors)
        {
            if (policy.MaxRecipients <= 0)
            {
                errors.Add($"{path}:DeliveryPolicy:MaxRecipients must be greater than zero.");
            }

            if (policy.MaxAttachmentCount < 0)
            {
                errors.Add($"{path}:DeliveryPolicy:MaxAttachmentCount cannot be negative.");
            }

            if (policy.MaxAttachmentBytes <= 0)
            {
                errors.Add($"{path}:DeliveryPolicy:MaxAttachmentBytes must be greater than zero.");
            }

            if (policy.MaxTotalAttachmentBytes <= 0)
            {
                errors.Add($"{path}:DeliveryPolicy:MaxTotalAttachmentBytes must be greater than zero.");
            }

            if (policy.MaxTotalAttachmentBytes < policy.MaxAttachmentBytes)
            {
                errors.Add($"{path}:DeliveryPolicy:MaxTotalAttachmentBytes cannot be less than MaxAttachmentBytes.");
            }

            if (policy.AllowedSenderDomains is null || policy.AllowedSenderDomains.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add($"{path}:DeliveryPolicy:AllowedSenderDomains cannot contain empty values.");
            }
        }

        private static void ValidateAuthentication(EmailAuthenticationOptions authentication, string path, ICollection<string> errors)
        {
            switch (authentication.Type)
            {
                case EmailAuthenticationType.Anonymous:
                    return;
                case EmailAuthenticationType.Login:
                    ValidateCredentials(authentication.Login, path, "Login", errors);
                    return;
                case EmailAuthenticationType.Plain:
                    ValidateCredentials(authentication.Plain, path, "Plain", errors);
                    return;
                case EmailAuthenticationType.OAuth:
                    if (string.IsNullOrWhiteSpace(authentication.OAuth.Username))
                    {
                        errors.Add($"{path}:Authentication:OAuth:Username is required for OAuth authentication.");
                    }

                    if (string.IsNullOrWhiteSpace(authentication.OAuth.AccessToken))
                    {
                        errors.Add($"{path}:Authentication:OAuth:AccessToken is required for OAuth authentication.");
                    }

                    return;
            }
        }

        private static void ValidateCredentials(EmailUsernamePasswordOptions credentials, string path, string authenticationType, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(credentials.Username))
            {
                errors.Add($"{path}:Authentication:{authenticationType}:Username is required for {authenticationType} authentication.");
            }

            if (string.IsNullOrWhiteSpace(credentials.Password))
            {
                errors.Add($"{path}:Authentication:{authenticationType}:Password is required for {authenticationType} authentication.");
            }
        }
    }
}
