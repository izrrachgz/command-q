namespace Common.Interfaces
{
    /// <summary>
    /// Should provide a way to run tasks in asynchronous thread safe environment
    /// </summary>
    public interface ITaskRunner
    {
        /// <summary>
        /// Should execute the tasks in asynchronous thread safe way
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RunTasksAsync(IEnumerable<Func<CancellationToken, Task>> tasks, CancellationToken cancellationToken = default);

        /// <summary>
        /// Should execute the tasks in asynchronous thread safe way
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RunTasksAsync(IEnumerable<Func<Task>> tasks, CancellationToken cancellationToken = default);

        /// <summary>
        /// Should execute the tasks in asynchronous thread safe way
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        Task RunTasksAsync(params Func<CancellationToken, Task>[] tasks);

        /// <summary>
        /// Should execute the tasks in asynchronous thread safe way
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        Task RunTasksAsync(params Func<Task>[] tasks);

        /// <summary>
        /// Should execute an asynchronous operation for each item in a thread safe way
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ForEachAsync<T>(IEnumerable<T> items, Func<T, CancellationToken, Task> task, CancellationToken cancellationToken = default);

        /// <summary>
        /// Should execute an asynchronous operation for each item in a thread safe way
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ForEachAsync<T>(IEnumerable<T> items, Func<T, Task> task, CancellationToken cancellationToken = default);
    }
}
