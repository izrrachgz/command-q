using Microsoft.Extensions.DependencyInjection;
using Application.Modules.Notifications.Services;

namespace Application.Modules.Notifications
{
    public static class NotificationsModuleExtensions
    {
        public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
        {
            services.AddScoped<IApplicationEmailService, ApplicationEmailService>();

            return services;
        }
    }
}
