using Common.Logging;
using Common.Constants;
using Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace Common.System
{
    /// <summary>
    /// Runs asynchronous operations with bounded concurrency and isolated state per execution.
    /// </summary>
    public sealed class TaskRunner : ITaskRunner
    {
        private readonly ILogger<TaskRunner> _logger;

        private readonly int _maxConcurrent;

        /// <summary>
        /// Creates a task runner with the requested maximum number of concurrent operations.
        /// </summary>
        public TaskRunner(ILogger<TaskRunner> logger, int maxConcurrent = TaskRunnerConstants.DefaultMaxConcurrency)
        {
            _logger = logger;
            _maxConcurrent = maxConcurrent > 0
                ? maxConcurrent
                : throw new ArgumentOutOfRangeException(nameof(maxConcurrent), maxConcurrent, "Max concurrency must be greater than zero.");

            _logger.TaskRunnerCreated(nameof(TaskRunner));
        }

        /// <summary>
        /// Executes task factories while limiting how many operations run at the same time.
        /// </summary>
        public async Task RunTasksAsync(IEnumerable<Func<CancellationToken, Task>> tasks, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            var taskFactories = tasks.ToList();
            _logger.ExecutingTasks(taskFactories.Count);

            await Parallel.ForEachAsync(
                taskFactories,
                new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = _maxConcurrent
                },
                async (taskFactory, taskCancellationToken) =>
                {
                    try
                    {
                        await taskFactory(taskCancellationToken);
                    }
                    catch (OperationCanceledException) when (taskCancellationToken.IsCancellationRequested)
                    {
                        _logger.TaskExecutionCancelled();
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.TaskExecutionFailed(ex);
                    }
                });
        }

        /// <summary>
        /// Executes task factories while limiting how many operations run at the same time.
        /// </summary>
        public async Task RunTasksAsync(IEnumerable<Func<Task>> tasks, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            await RunTasksAsync(tasks.Select<Func<Task>, Func<CancellationToken, Task>>(task => _ => task()), cancellationToken);
        }

        /// <summary>
        /// Executes task factories while limiting how many operations run at the same time.
        /// </summary>
        public async Task RunTasksAsync(params Func<CancellationToken, Task>[] tasks)
        {
            await RunTasksAsync((IEnumerable<Func<CancellationToken, Task>>)tasks);
        }

        /// <summary>
        /// Executes task factories while limiting how many operations run at the same time.
        /// </summary>
        public async Task RunTasksAsync(params Func<Task>[] tasks)
        {
            await RunTasksAsync((IEnumerable<Func<Task>>)tasks);
        }

        /// <summary>
        /// Executes an asynchronous operation for each item while limiting concurrency.
        /// </summary>
        public async Task ForEachAsync<T>(IEnumerable<T> items, Func<T, CancellationToken, Task> task, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(task);

            await RunTasksAsync(items.Select<T, Func<CancellationToken, Task>>(item => token => task(item, token)), cancellationToken);
        }

        /// <summary>
        /// Executes an asynchronous operation for each item while limiting concurrency.
        /// </summary>
        public async Task ForEachAsync<T>(IEnumerable<T> items, Func<T, Task> task, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(task);

            await ForEachAsync(items, (item, _) => task(item), cancellationToken);
        }
    }
}
