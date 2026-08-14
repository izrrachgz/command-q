using System.Diagnostics;
using Email.Outbox.Models;
using Email.Worker.Logging;
using Email.Worker.Telemetry;
using Email.Outbox.Abstractions;
using System.Diagnostics.Metrics;
using Email.Worker.Configuration;
using Microsoft.Extensions.Options;

namespace Email.Worker.Services
{
    internal sealed class EmailOutboxBackgroundService(
        IEmailOutboxProcessor processor,
        TimeProvider timeProvider,
        IOptionsMonitor<EmailWorkerOptions> options,
        ILogger<EmailOutboxBackgroundService> logger)
        : BackgroundService
    {
        private readonly string _workerId = $"{Environment.MachineName}:{Guid.NewGuid():N}";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using IDisposable? configurationSubscription = options.OnChange(currentOptions => logger.ConfigurationReloaded(
                    currentOptions.Enabled,
                    currentOptions.BatchSize,
                    currentOptions.PollIntervalSeconds,
                    currentOptions.LeaseSeconds,
                    currentOptions.ErrorDelaySeconds));
                bool? wasEnabled = null;

                while (!stoppingToken.IsCancellationRequested)
                {
                    EmailWorkerOptions currentOptions = options.CurrentValue;

                    if (!currentOptions.Enabled)
                    {
                        if (wasEnabled is not false)
                        {
                            logger.Disabled();
                        }

                        wasEnabled = false;

                        await Task.Delay(TimeSpan.FromSeconds(currentOptions.PollIntervalSeconds), timeProvider, stoppingToken);
                        continue;
                    }

                    if (wasEnabled is not true)
                    {
                        logger.Started(_workerId, currentOptions.BatchSize, currentOptions.PollIntervalSeconds);
                        wasEnabled = true;
                    }

                    long startedAt = Stopwatch.GetTimestamp();
                    using Activity? pollActivity = EmailWorkerTelemetry.ActivitySource.StartActivity("email.worker.poll");
                    using Activity? batchActivity = EmailWorkerTelemetry.ActivitySource.StartActivity("email.worker.batch");

                    try
                    {
                        var result = await processor.ProcessBatchAsync(
                            new EmailOutboxClaimRequest(
                                currentOptions.BatchSize,
                                _workerId,
                                TimeSpan.FromSeconds(currentOptions.LeaseSeconds),
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
                            batchActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, "success");
                            pollActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, "success");
                            EmailWorkerTelemetry.PollDuration.Record(
                                Stopwatch.GetElapsedTime(startedAt).TotalSeconds,
                                new TagList { { EmailWorkerTelemetry.OutcomeTag, "success" } });
                            continue;
                        }

                        await Task.Delay(
                            TimeSpan.FromSeconds(currentOptions.PollIntervalSeconds),
                            timeProvider,
                            stoppingToken);
                        pollActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, "empty");
                        EmailWorkerTelemetry.PollDuration.Record(
                            Stopwatch.GetElapsedTime(startedAt).TotalSeconds,
                            new TagList { { EmailWorkerTelemetry.OutcomeTag, "empty" } });
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception exception)
                    {
                        batchActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, "failure");
                        batchActivity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
                        pollActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, "failure");
                        EmailWorkerTelemetry.PollDuration.Record(
                            Stopwatch.GetElapsedTime(startedAt).TotalSeconds,
                            new TagList { { EmailWorkerTelemetry.OutcomeTag, "failure" } });
                        logger.PollFailed(exception);
                        await Task.Delay(
                            TimeSpan.FromSeconds(currentOptions.ErrorDelaySeconds),
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
