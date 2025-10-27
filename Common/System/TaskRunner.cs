using Common.Constants;
using Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Common.System
{
    public class TaskRunner : ITaskRunner
    {
        private readonly SemaphoreSlim _semaphore;

        private readonly ILogger<TaskRunner> _logger;

        public TaskRunner(ILogger<TaskRunner> logger, int maxConcurrent = 4)
        {
            _logger = logger;
            _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        }

        public async Task RunTasksAsync(IEnumerable<Func<CancellationToken, Task>> tasks, CancellationToken cancellationToken = default)
        {
            var runningTasks = new List<Task>();

            _logger.LogDebug(@"Executing {Count} tasks.", tasks.Count());

            foreach (var taskFactory in tasks)
            {
                await _semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        await taskFactory(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine(ProcessConstants.ProcessCancelled);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");

                        _logger.LogError(@"Error executing task : {exception}", ex);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, cancellationToken);

                runningTasks.Add(task);
            }

            await Task.WhenAll(runningTasks);
        }
    }
}
