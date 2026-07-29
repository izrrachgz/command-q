using Common.Interfaces;
using Common.System;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions
{
    public static class TaskRunnerExtensions
    {
        public static IServiceCollection AddTaskRunner(this IServiceCollection services)
        {
            services.AddScoped<TaskRunner>();
            services.AddScoped<ITaskRunner>(serviceProvider => serviceProvider.GetRequiredService<TaskRunner>());

            return services;
        }
    }
}
