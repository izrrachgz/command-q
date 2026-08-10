using Microsoft.Extensions.Options;

namespace Email.Outbox.Persistence.Configuration
{
    internal sealed class EmailOutboxPersistenceOptionsValidator
        : IValidateOptions<EmailOutboxPersistenceOptions>
    {
        public ValidateOptionsResult Validate(string? name, EmailOutboxPersistenceOptions options)
        {
            if (!Enum.IsDefined(options.Engine))
            {
                return ValidateOptionsResult.Fail("Email outbox persistence Engine is not supported.");
            }

            return string.IsNullOrWhiteSpace(options.ConnectionString)
                ? ValidateOptionsResult.Fail("Email outbox persistence ConnectionString is required.")
                : ValidateOptionsResult.Success;
        }
    }
}
