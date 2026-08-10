using MimeKit;
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

            if (string.IsNullOrWhiteSpace(options.Smtp.Host))
            {
                errors.Add("Email:Smtp:Host is required.");
            }

            if (options.Smtp.Port is < 1 or > 65535)
            {
                errors.Add("Email:Smtp:Port must be between 1 and 65535.");
            }

            if (!Enum.IsDefined(options.Smtp.Security))
            {
                errors.Add("Email:Smtp:Security is invalid.");
            }

            if (options.Smtp.TimeoutMilliseconds <= 0)
            {
                errors.Add("Email:Smtp:TimeoutMilliseconds must be greater than zero.");
            }

            if (!Enum.IsDefined(options.Authentication.Type))
            {
                errors.Add("Email:Authentication:Type is invalid.");
            }
            else
            {
                ValidateAuthentication(options.Authentication, errors);

                if (options.Authentication.Type != EmailAuthenticationType.Anonymous &&
                    options.Smtp.Security is not EmailSecurityMode.StartTls and not EmailSecurityMode.SslOnConnect)
                {
                    errors.Add("Authenticated SMTP delivery requires Email:Smtp:Security to be StartTls or SslOnConnect.");
                }
            }

            if (!MailboxAddress.TryParse(options.Sender.Address, out _))
            {
                errors.Add("Email:Sender:Address must be a valid mailbox address.");
            }

            ValidateDeliveryPolicy(options.DeliveryPolicy, errors);

            return errors.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(errors);
        }

        private static void ValidateDeliveryPolicy(EmailDeliveryPolicyOptions policy, ICollection<string> errors)
        {
            if (policy.MaxRecipients <= 0)
            {
                errors.Add("Email:DeliveryPolicy:MaxRecipients must be greater than zero.");
            }

            if (policy.MaxAttachmentCount < 0)
            {
                errors.Add("Email:DeliveryPolicy:MaxAttachmentCount cannot be negative.");
            }

            if (policy.MaxAttachmentBytes <= 0)
            {
                errors.Add("Email:DeliveryPolicy:MaxAttachmentBytes must be greater than zero.");
            }

            if (policy.MaxTotalAttachmentBytes <= 0)
            {
                errors.Add("Email:DeliveryPolicy:MaxTotalAttachmentBytes must be greater than zero.");
            }

            if (policy.MaxTotalAttachmentBytes < policy.MaxAttachmentBytes)
            {
                errors.Add("Email:DeliveryPolicy:MaxTotalAttachmentBytes cannot be less than MaxAttachmentBytes.");
            }

            if (policy.AllowedSenderDomains is null || policy.AllowedSenderDomains.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add("Email:DeliveryPolicy:AllowedSenderDomains cannot contain empty values.");
            }
        }

        private static void ValidateAuthentication(EmailAuthenticationOptions authentication, ICollection<string> errors)
        {
            switch (authentication.Type)
            {
                case EmailAuthenticationType.Anonymous:
                    return;
                case EmailAuthenticationType.Login:
                    ValidateCredentials(authentication.Login, "Login", errors);
                    return;
                case EmailAuthenticationType.Plain:
                    ValidateCredentials(authentication.Plain, "Plain", errors);
                    return;
                case EmailAuthenticationType.OAuth:
                    if (string.IsNullOrWhiteSpace(authentication.OAuth.Username))
                    {
                        errors.Add("Email:Authentication:OAuth:Username is required for OAuth authentication.");
                    }

                    if (string.IsNullOrWhiteSpace(authentication.OAuth.AccessToken))
                    {
                        errors.Add("Email:Authentication:OAuth:AccessToken is required for OAuth authentication.");
                    }

                    return;
            }
        }

        private static void ValidateCredentials(
            EmailUsernamePasswordOptions credentials,
            string authenticationType,
            ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(credentials.Username))
            {
                errors.Add($"Email:Authentication:{authenticationType}:Username is required for {authenticationType} authentication.");
            }

            if (string.IsNullOrWhiteSpace(credentials.Password))
            {
                errors.Add($"Email:Authentication:{authenticationType}:Password is required for {authenticationType} authentication.");
            }
        }
    }
}
