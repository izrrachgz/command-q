using FluentValidation;
using Application.Extensions;
using Common.Models.Response;
using Application.Modules.Notifications.Dtos;
using Application.Modules.Notifications.Models;
using Microsoft.Extensions.DependencyInjection;
using Application.Modules.Notifications.Commands;
using Application.Modules.Notifications.Services;
using Application.Modules.Notifications.Queries;
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
        /// Registers Notifications services, account email requests and validators.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
        {
            services.AddScoped<IApplicationEmailService, ApplicationEmailService>();
            services.TryAddScoped<IEmailProfileSelectionPolicy, DefaultEmailProfileSelectionPolicy>();

            services.AddApplicationRequest<QueueEmailCommand, QueuedEmailResult, QueueEmailCommandHandler>();
            services.AddApplicationRequest<
                GetAccountEmailsQuery,
                PagedResponse<QueuedEmailModel>,
                GetAccountEmailsQueryHandler>();
            services.AddApplicationRequest<
                SearchEmailsQuery,
                PagedResponse<QueuedEmailModel>,
                SearchEmailsQueryHandler>();
            services.AddApplicationRequest<
                GetEmailDetailQuery,
                QueuedEmailDetailModel?,
                GetEmailDetailQueryHandler>();

            services.AddScoped<IValidator<ApplicationEmailRequest>, ApplicationEmailRequestValidator>();
            services.AddScoped<IValidator<QueueEmailCommand>, QueueEmailCommandValidator>();
            services.AddScoped<IValidator<GetAccountEmailsQuery>, GetAccountEmailsQueryValidator>();
            services.AddScoped<IValidator<SearchEmailsQuery>, SearchEmailsQueryValidator>();

            return services;
        }
    }
}
