using Common.System;
using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions
{
    /// <summary>
    /// Registers task runner services.
    /// </summary>
    public static class TaskRunnerExtensions
    {
        /// <summary>
        /// Registers <see cref="TaskRunner"/> and <see cref="ITaskRunner"/> with scoped lifetime.
        /// </summary>
        public static IServiceCollection AddTaskRunner(this IServiceCollection services)
        {
            services.AddScoped<TaskRunner>();
            services.AddScoped<ITaskRunner>(serviceProvider => serviceProvider.GetRequiredService<TaskRunner>());

            return services;
        }
    }
}
