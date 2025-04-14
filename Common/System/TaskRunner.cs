using Common.Constants;
using Common.Interfaces;

namespace Common.System
{
    public class TaskRunner : ITaskRunner
    {
        private readonly SemaphoreSlim _semaphore;

        public TaskRunner(int maxConcurrent = 4)
        {
            _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        }

        public async Task RunTasksAsync(IEnumerable<Func<CancellationToken, Task>> tasks, CancellationToken cancellationToken = default)
        {
            var runningTasks = new List<Task>();

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
