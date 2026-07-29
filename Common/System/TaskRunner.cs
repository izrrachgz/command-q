using Common.Constants;
using Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Common.System
{
    public class TaskRunner : ITaskRunner
    {
        private readonly ILogger<TaskRunner> _logger;

        private readonly int _maxConcurrent;

        public TaskRunner(ILogger<TaskRunner> logger, int maxConcurrent = TaskRunnerConstants.DefaultMaxConcurrency)
        {
            _logger = logger;
            _maxConcurrent = maxConcurrent > 0
                ? maxConcurrent
                : throw new ArgumentOutOfRangeException(nameof(maxConcurrent), maxConcurrent, "Max concurrency must be greater than zero.");

            _logger.LogDebug(InstanceConstants.InstanceCreated, nameof(TaskRunner));
        }

        public async Task RunTasksAsync(IEnumerable<Func<CancellationToken, Task>> tasks, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            var taskFactories = tasks.ToList();
            using var semaphore = new SemaphoreSlim(_maxConcurrent, _maxConcurrent);
            var runningTasks = new List<Task>(taskFactories.Count);

            _logger.LogDebug(ProcessConstants.ExecutingTasks, taskFactories.Count);

            foreach (var taskFactory in taskFactories)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        await taskFactory(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine(ProcessConstants.CancellationHasBeenRequested);

                        _logger.LogDebug(ProcessConstants.CancellationHasBeenRequested);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"{ErrorConstants.AnExceptionHasOccurred}: {ex.Message}");

                        _logger.LogError(ex, ErrorConstants.AnExceptionHasOccurred);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                runningTasks.Add(task);
            }

            await Task.WhenAll(runningTasks);
        }

        public async Task RunTasksAsync(IEnumerable<Func<Task>> tasks, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            await RunTasksAsync(tasks.Select<Func<Task>, Func<CancellationToken, Task>>(task => _ => task()), cancellationToken);
        }

        public async Task RunTasksAsync(params Func<CancellationToken, Task>[] tasks)
        {
            await RunTasksAsync((IEnumerable<Func<CancellationToken, Task>>)tasks);
        }

        public async Task RunTasksAsync(params Func<Task>[] tasks)
        {
            await RunTasksAsync((IEnumerable<Func<Task>>)tasks);
        }

        public async Task ForEachAsync<T>(IEnumerable<T> items, Func<T, CancellationToken, Task> task, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(task);

            await RunTasksAsync(items.Select<T, Func<CancellationToken, Task>>(item => token => task(item, token)), cancellationToken);
        }

        public async Task ForEachAsync<T>(IEnumerable<T> items, Func<T, Task> task, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(task);

            await ForEachAsync(items, (item, _) => task(item), cancellationToken);
        }
    }
}
