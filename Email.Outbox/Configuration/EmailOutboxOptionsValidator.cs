using Email.Configuration;
using Email.Outbox.Constants;
using Microsoft.Extensions.Options;

namespace Email.Outbox.Configuration
{
    internal sealed class EmailOutboxOptionsValidator : IValidateOptions<EmailOutboxOptions>
    {
        public ValidateOptionsResult Validate(string? name, EmailOutboxOptions options)
        {
            var failures = new List<string>();

            EmailDeliveryPolicyOptionsValidator.Validate(
                options.DeliveryPolicy,
                $"{EmailOutboxConstants.ConfigurationSection}:DeliveryPolicy",
                failures);

            if (options.MaxAttempts <= 0)
            {
                failures.Add($"{EmailOutboxConstants.ConfigurationSection}:MaxAttempts must be greater than zero.");
            }

            if (options.BaseRetryDelaySeconds <= 0)
            {
                failures.Add($"{EmailOutboxConstants.ConfigurationSection}:BaseRetryDelaySeconds must be greater than zero.");
            }

            if (options.MaxRetryDelaySeconds < options.BaseRetryDelaySeconds)
            {
                failures.Add($"{EmailOutboxConstants.ConfigurationSection}:MaxRetryDelaySeconds must be greater than or equal to BaseRetryDelaySeconds.");
            }

            if (options.MaxErrorLength <= 0)
            {
                failures.Add($"{EmailOutboxConstants.ConfigurationSection}:MaxErrorLength must be greater than zero.");
            }

            if (options.MaxPayloadBytes <= 0)
            {
                failures.Add($"{EmailOutboxConstants.ConfigurationSection}:MaxPayloadBytes must be greater than zero.");
            }

            return failures.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(failures);
        }
    }
}
