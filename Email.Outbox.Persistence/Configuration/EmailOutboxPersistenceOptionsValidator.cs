using Microsoft.Extensions.Options;

namespace Email.Outbox.Persistence.Configuration
{
    internal sealed class EmailOutboxPersistenceOptionsValidator
        : IValidateOptions<EmailOutboxPersistenceOptions>
    {
        public ValidateOptionsResult Validate(
            string? name,
            EmailOutboxPersistenceOptions options)
        {
            if (!Enum.IsDefined(options.Engine))
            {
                return ValidateOptionsResult.Fail("EmailOutbox:Persistence:Engine is not supported.");
            }

            return string.IsNullOrWhiteSpace(options.ConnectionString)
                ? ValidateOptionsResult.Fail("EmailOutbox:Persistence:ConnectionString is required.")
                : ValidateOptionsResult.Success;
        }
    }
}
