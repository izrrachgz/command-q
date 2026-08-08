using Email.Outbox.Models;
using Email.Worker.Logging;
using Email.Outbox.Abstractions;
using Email.Worker.Configuration;
using Microsoft.Extensions.Options;

namespace Email.Worker.Services
{
    internal sealed class EmailOutboxBackgroundService(
        IEmailOutboxProcessor processor,
        TimeProvider timeProvider,
        IOptions<EmailWorkerOptions> options,
        ILogger<EmailOutboxBackgroundService> logger)
        : BackgroundService
    {
        private readonly EmailWorkerOptions _options = options.Value;
        private readonly string _workerId = $"{Environment.MachineName}:{Guid.NewGuid():N}";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                logger.Disabled();
                return;
            }

            logger.Started(_workerId, _options.BatchSize, _options.PollIntervalSeconds);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = await processor.ProcessBatchAsync(
                            new EmailOutboxClaimRequest(
                                _options.BatchSize,
                                _workerId,
                                TimeSpan.FromSeconds(_options.LeaseSeconds),
                                timeProvider.GetUtcNow().UtcDateTime),
                            stoppingToken);

                        if (result.ClaimedCount > 0)
                        {
                            logger.BatchCompleted(
                                result.ClaimedCount,
                                result.SentCount,
                                result.RetryCount,
                                result.DeadLetterCount,
                                result.ReleasedCount);
                            continue;
                        }

                        await Task.Delay(
                            TimeSpan.FromSeconds(_options.PollIntervalSeconds),
                            timeProvider,
                            stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception exception)
                    {
                        logger.PollFailed(exception);
                        await Task.Delay(
                            TimeSpan.FromSeconds(_options.ErrorDelaySeconds),
                            timeProvider,
                            stoppingToken);
                    }
                }
            }
            finally
            {
                logger.Stopped(_workerId);
            }
        }
    }
}
