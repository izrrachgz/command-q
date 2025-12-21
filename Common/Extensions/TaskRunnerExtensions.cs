using Common.System;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions
{
    public static class TaskRunnerExtensions
    {
        public static void AddTaskRunner(this IServiceCollection services)
        {
            services.AddScoped<TaskRunner>();
        }
    }
}
