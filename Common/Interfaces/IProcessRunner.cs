using Common.Models.System;

namespace Common.Interfaces
{
    /// <summary>
    /// Should provide a way to run commands/tasks/processes against a operating system
    /// </summary>
    public interface IProcessRunner
    {
        /// <summary>
        /// Should execute the command/task/process in parallel asynchronous thread safe way
        /// </summary>
        /// <param name="command">Command/program to be activated</param>
        /// <param name="arguments">Arguments to include</param>
        /// <param name="cancellationToken">Cancellation token to interrupt execution</param>
        /// <returns></returns>
        Task<ProcessResult> RunCommandAsync(string command, string arguments = "", CancellationToken cancellationToken = default);
    }
}
