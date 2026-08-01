using Common.System;
using Common.Extensions;
using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Test.Modules.Common.System
{
    public class TaskRunnerTest
    {
        [Fact]
        public void Constructor_Should_Reject_Invalid_Max_Concurrency()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TaskRunner(NullLogger<TaskRunner>.Instance, 0));
        }

        [Fact]
        public void AddTaskRunner_Should_Register_Interface()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddTaskRunner();

            using var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<ITaskRunner>());
        }

        [Fact]
        public async Task RunTasksAsync_Should_Respect_Max_Concurrency()
        {
            var runner = new TaskRunner(NullLogger<TaskRunner>.Instance, maxConcurrent: 2);
            int current = 0;
            int maxObserved = 0;

            IEnumerable<Func<CancellationToken, Task>> tasks = Enumerable.Range(0, 6)
                .Select(_ => new Func<CancellationToken, Task>(async cancellationToken =>
                {
                    int running = Interlocked.Increment(ref current);
                    SetMaxObserved(ref maxObserved, running);

                    await Task.Delay(25, cancellationToken);

                    Interlocked.Decrement(ref current);
                }));

            await runner.RunTasksAsync(tasks, TestContext.Current.CancellationToken);

            Assert.Equal(2, maxObserved);
        }

        [Fact]
        public async Task RunTasksAsync_Should_Enumerate_Tasks_Once()
        {
            var runner = new TaskRunner(NullLogger<TaskRunner>.Instance, maxConcurrent: 2);
            int enumerationCount = 0;

            IEnumerable<Func<CancellationToken, Task>> tasks = CreateTasks();

            await runner.RunTasksAsync(tasks, TestContext.Current.CancellationToken);

            Assert.Equal(1, enumerationCount);

            IEnumerable<Func<CancellationToken, Task>> CreateTasks()
            {
                enumerationCount++;

                yield return _ => Task.CompletedTask;
            }
        }

        [Fact]
        public async Task RunTasksAsync_Should_Run_Task_Delegates()
        {
            var runner = new TaskRunner(NullLogger<TaskRunner>.Instance, maxConcurrent: 2);
            int completed = 0;

            await runner.RunTasksAsync(
                () =>
                {
                    Interlocked.Increment(ref completed);

                    return Task.CompletedTask;
                },
                () =>
                {
                    Interlocked.Increment(ref completed);

                    return Task.CompletedTask;
                });

            Assert.Equal(2, completed);
        }

        [Fact]
        public async Task ForEachAsync_Should_Run_Task_For_Each_Item()
        {
            var runner = new TaskRunner(NullLogger<TaskRunner>.Instance, maxConcurrent: 2);
            int total = 0;

            await runner.ForEachAsync([1, 2, 3], item =>
            {
                Interlocked.Add(ref total, item);

                return Task.CompletedTask;
            }, TestContext.Current.CancellationToken);

            Assert.Equal(6, total);
        }

        [Fact]
        public async Task ForEachAsync_Should_Respect_Max_Concurrency()
        {
            var runner = new TaskRunner(NullLogger<TaskRunner>.Instance, maxConcurrent: 2);
            int current = 0;
            int maxObserved = 0;

            await runner.ForEachAsync(Enumerable.Range(0, 6), async (_, cancellationToken) =>
            {
                int running = Interlocked.Increment(ref current);
                SetMaxObserved(ref maxObserved, running);

                await Task.Delay(25, cancellationToken);

                Interlocked.Decrement(ref current);
            }, TestContext.Current.CancellationToken);

            Assert.Equal(2, maxObserved);
        }

        private static void SetMaxObserved(ref int maxObserved, int running)
        {
            int snapshot;

            do
            {
                snapshot = maxObserved;

                if (running <= snapshot)
                    return;
            }
            while (Interlocked.CompareExchange(ref maxObserved, running, snapshot) != snapshot);
        }
    }
}
