using NodaTime;
using Email.Time;
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
                    using Activity? pollActivity = EmailWorkerTelemetry.ActivitySource.StartActivity(EmailWorkerTelemetry.PollActivityName);
                    using Activity? batchActivity = EmailWorkerTelemetry.ActivitySource.StartActivity(EmailWorkerTelemetry.BatchActivityName);

                    try
                    {
                        var result = await processor.ProcessBatchAsync(
                            new EmailOutboxClaimRequest(
                                currentOptions.BatchSize,
                                _workerId,
                                Duration.FromSeconds(currentOptions.LeaseSeconds),
                                timeProvider.GetCurrentInstant()),
                            stoppingToken);

                        if (result.ClaimedCount > 0)
                        {
                            logger.BatchCompleted(
                                result.ClaimedCount,
                                result.SentCount,
                                result.RetryCount,
                                result.DeadLetterCount,
                                result.ReleasedCount);
                            batchActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, EmailWorkerTelemetry.SuccessOutcome);
                            pollActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, EmailWorkerTelemetry.SuccessOutcome);
                            EmailWorkerTelemetry.PollDuration.Record(
                                Stopwatch.GetElapsedTime(startedAt).TotalSeconds,
                                new TagList { { EmailWorkerTelemetry.OutcomeTag, EmailWorkerTelemetry.SuccessOutcome } });
                            continue;
                        }

                        await Task.Delay(
                            TimeSpan.FromSeconds(currentOptions.PollIntervalSeconds),
                            timeProvider,
                            stoppingToken);
                        pollActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, EmailWorkerTelemetry.EmptyOutcome);
                        EmailWorkerTelemetry.PollDuration.Record(
                            Stopwatch.GetElapsedTime(startedAt).TotalSeconds,
                            new TagList { { EmailWorkerTelemetry.OutcomeTag, EmailWorkerTelemetry.EmptyOutcome } });
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception exception)
                    {
                        batchActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, EmailWorkerTelemetry.FailureOutcome);
                        batchActivity?.SetStatus(ActivityStatusCode.Error);
                        pollActivity?.SetTag(EmailWorkerTelemetry.OutcomeTag, EmailWorkerTelemetry.FailureOutcome);
                        pollActivity?.SetStatus(ActivityStatusCode.Error);
                        EmailWorkerTelemetry.PollDuration.Record(
                            Stopwatch.GetElapsedTime(startedAt).TotalSeconds,
                            new TagList { { EmailWorkerTelemetry.OutcomeTag, EmailWorkerTelemetry.FailureOutcome } });
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
