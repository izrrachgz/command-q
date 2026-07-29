namespace Common.Interfaces
{
    /// <summary>
    /// Runs asynchronous operations with a bounded concurrency limit.
    /// </summary>
    public interface ITaskRunner
    {
        /// <summary>
        /// Executes task factories while limiting how many operations run at the same time.
        /// </summary>
        /// <param name="tasks">Task factories that receive the shared cancellation token.</param>
        /// <param name="cancellationToken">Token used to stop scheduling and passed to each task factory.</param>
        Task RunTasksAsync(IEnumerable<Func<CancellationToken, Task>> tasks, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes task factories while limiting how many operations run at the same time.
        /// </summary>
        /// <param name="tasks">Task factories that do not require a cancellation token.</param>
        /// <param name="cancellationToken">Token used to stop scheduling new work.</param>
        Task RunTasksAsync(IEnumerable<Func<Task>> tasks, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes task factories while limiting how many operations run at the same time.
        /// </summary>
        /// <param name="tasks">Task factories that receive the shared cancellation token.</param>
        Task RunTasksAsync(params Func<CancellationToken, Task>[] tasks);

        /// <summary>
        /// Executes task factories while limiting how many operations run at the same time.
        /// </summary>
        /// <param name="tasks">Task factories that do not require a cancellation token.</param>
        Task RunTasksAsync(params Func<Task>[] tasks);

        /// <summary>
        /// Executes an asynchronous operation for each item while limiting concurrency.
        /// </summary>
        /// <typeparam name="T">Type of the item being processed.</typeparam>
        /// <param name="items">Items to process.</param>
        /// <param name="task">Operation to execute for each item.</param>
        /// <param name="cancellationToken">Token used to stop scheduling and passed to each task invocation.</param>
        Task ForEachAsync<T>(IEnumerable<T> items, Func<T, CancellationToken, Task> task, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes an asynchronous operation for each item while limiting concurrency.
        /// </summary>
        /// <typeparam name="T">Type of the item being processed.</typeparam>
        /// <param name="items">Items to process.</param>
        /// <param name="task">Operation to execute for each item.</param>
        /// <param name="cancellationToken">Token used to stop scheduling new work.</param>
        Task ForEachAsync<T>(IEnumerable<T> items, Func<T, Task> task, CancellationToken cancellationToken = default);
    }
}
