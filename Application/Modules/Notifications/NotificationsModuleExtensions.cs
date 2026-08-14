using MediatR;
using FluentValidation;
using Application.Modules.Notifications.Models;
using Microsoft.Extensions.DependencyInjection;
using Application.Modules.Notifications.Commands;
using Application.Modules.Notifications.Services;
using Application.Modules.Notifications.Validation;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.Modules.Notifications
{
    /// <summary>
    /// Provides dependency-injection registration for the Notifications module.
    /// </summary>
    public static class NotificationsModuleExtensions
    {
        /// <summary>
        /// Registers the Notifications application service, command handler and validators.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
        {
            services.AddScoped<IApplicationEmailService, ApplicationEmailService>();
            services.TryAddScoped<IEmailProfileSelectionPolicy, DefaultEmailProfileSelectionPolicy>();

            services.AddTransient<IRequestHandler<QueueEmailCommand, QueuedEmailResult>, QueueEmailCommandHandler>();

            services.AddScoped<IValidator<ApplicationEmailRequest>, ApplicationEmailRequestValidator>();
            services.AddScoped<IValidator<QueueEmailCommand>, QueueEmailCommandValidator>();

            return services;
        }
    }
}
