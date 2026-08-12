using MediatR;
using Common.Cqrs;
using FluentValidation;
using Domain.Modules.Health;
using Application.Abstractions;
using Application.Modules.Health.Dtos;
using Application.Modules.Health.Queries;
using Microsoft.Extensions.Configuration;
using Application.Modules.Health.Commands;
using Application.Modules.Health.Mappings;
using Application.Modules.Health.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health
{
    /// <summary>
    /// Provides dependency-injection registration for the Health module.
    /// </summary>
    public static class HealthModuleExtensions
    {
        /// <summary>
        /// Registers the Health application handlers, validators, mapper and health checks.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The host configuration used by health checks.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddHealthModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureAppHealthChecks(configuration);

            services.AddTransient<IRequestHandler<SaveDatabaseDataCommand, HealthCheckResult>, SaveDatabaseDataCommandHandler>();
            services.AddTransient<IRequestHandler<SaveLocalStorageDataCommand, HealthCheckResult>, SaveLocalStorageDataCommandHandler>();
            services.AddTransient<IRequestHandler<CheckInternetConnectionQuery, HealthCheckResult>, CheckInternetConnectionQueryHandler>();
            services.AddTransient<IRequestHandler<GetDatabaseDataQuery, HealthCheckResult>, GetDatabaseDataQueryHandler>();

            services.AddScoped<IValidator<SaveDatabaseDataCommand>, SaveDatabaseDataCommandValidator>();
            services.AddScoped<IValidator<SaveLocalStorageDataCommand>, SaveLocalStorageDataCommandValidator>();
            services.AddScoped<IValidator<CheckInternetConnectionQuery>, CheckInternetConnectionQueryValidator>();
            services.AddScoped<IValidator<GetDatabaseDataQuery>, GetDatabaseDataQueryValidator>();

            services.AddSingleton<IMapper<HealthCheckLog, HealthCheckResponseModel>, HealthCheckMapper>();

            return services;
        }
    }
}
