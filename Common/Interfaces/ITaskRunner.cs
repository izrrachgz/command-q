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
    }
}
