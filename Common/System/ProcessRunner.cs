using Common.Constants;
using Common.Interfaces;
using System.Diagnostics;
using Common.Models.System;

namespace Common.System
{
    public class ProcessRunner : IProcessRunner
    {
        private readonly SemaphoreSlim _semaphore;

        public ProcessRunner(int maxParallel = 4)
        {
            _semaphore = new SemaphoreSlim(maxParallel, maxParallel);
        }

        public async Task<ProcessResult> RunCommandAsync(string command, string arguments = "", CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            var result = new ProcessResult();

            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);

                await Task.WhenAll(stdOutTask, stdErrTask, process.WaitForExitAsync(cancellationToken));

                result = new ProcessResult
                {
                    StdOutput = await stdOutTask,
                    StdError = await stdErrTask,
                    ExitCode = process.ExitCode
                };

                return result;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(ProcessConstants.ProcessCancelled);
            }
            finally
            {
                _semaphore.Release();
            }
            return result;
        }
    }

}
