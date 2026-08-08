using Microsoft.Extensions.Options;

namespace Email.Worker.Configuration
{
    internal sealed class EmailWorkerOptionsValidator : IValidateOptions<EmailWorkerOptions>
    {
        public ValidateOptionsResult Validate(string? name, EmailWorkerOptions options)
        {
            var failures = new List<string>();

            if (options.BatchSize <= 0)
            {
                failures.Add("EmailOutbox:Worker:BatchSize must be greater than zero.");
            }

            if (options.PollIntervalSeconds <= 0)
            {
                failures.Add("EmailOutbox:Worker:PollIntervalSeconds must be greater than zero.");
            }

            if (options.LeaseSeconds <= 0)
            {
                failures.Add("EmailOutbox:Worker:LeaseSeconds must be greater than zero.");
            }

            if (options.ErrorDelaySeconds <= 0)
            {
                failures.Add("EmailOutbox:Worker:ErrorDelaySeconds must be greater than zero.");
            }

            return failures.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(failures);
        }
    }
}
