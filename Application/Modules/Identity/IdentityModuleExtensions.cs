using MediatR;
using Common.Cqrs;
using FluentValidation;
using Common.Models.Response;
using Domain.Modules.Identity;
using Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Application.Modules.Identity.Accounts.Dtos;
using Application.Modules.Identity.Accounts.Queries;
using Application.Modules.Identity.Accounts.Commands;
using Application.Modules.Identity.Accounts.Mappings;
using Application.Modules.Identity.Accounts.Services;
using Application.Modules.Identity.AccountActivity.Dtos;
using Application.Modules.Identity.AccountActivity.Queries;
using Application.Modules.Identity.AccountActivity.Commands;
using Application.Modules.Identity.AccountActivity.Mappings;
using Application.Modules.Identity.AccountActivity.Services;

namespace Application.Modules.Identity
{
    /// <summary>
    /// Provides dependency-injection registration for the Identity module.
    /// </summary>
    public static class IdentityModuleExtensions
    {
        /// <summary>
        /// Registers the Identity application services, handlers, validators and mappers.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddIdentityModule(this IServiceCollection services)
        {
            services.AddScoped<AccountService>();
            services.AddScoped<IAccountActivityRecorder, AccountActivityRecorder>();

            services.AddTransient<IRequestHandler<CreateAccountCommand, BasicResponse>, CreateAccountCommandHandler>();
            services.AddTransient<IRequestHandler<GetAccountByIdentityQuery, DetailResponse<AccountModel>>, GetAccountByIdentityQueryHandler>();
            services.AddTransient<IRequestHandler<GetAccountByIdQuery, DetailResponse<AccountModel>>, GetAccountByIdQueryHandler>();
            services.AddTransient<IRequestHandler<GetAccountByUsernameQuery, DetailResponse<AccountModel>>, GetAccountByUsernameQueryHandler>();
            services.AddTransient<IRequestHandler<RecordAccountActivityCommand, BasicResponse>, RecordAccountActivityCommandHandler>();
            services.AddTransient<IRequestHandler<GetAccountActivityQuery, PagedResponse<AccountActivityModel>>, GetAccountActivityQueryHandler>();

            services.AddScoped<IValidator<CreateAccountCommand>, CreateAccountCommandValidator>();
            services.AddScoped<IValidator<GetAccountByIdentityQuery>, GetAccountByIdentityQueryValidator>();
            services.AddScoped<IValidator<GetAccountByIdQuery>, GetAccountByIdQueryValidator>();
            services.AddScoped<IValidator<GetAccountByUsernameQuery>, GetAccountByUsernameQueryValidator>();
            services.AddScoped<IValidator<RecordAccountActivityCommand>, RecordAccountActivityCommandValidator>();
            services.AddScoped<IValidator<GetAccountActivityQuery>, GetAccountActivityQueryValidator>();

            services.AddSingleton<IMapper<Account, AccountModel>, AccountMapper>();
            services.AddSingleton<IMapper<Domain.Modules.Identity.AccountActivity, AccountActivityModel>, AccountActivityMapper>();

            return services;
        }
    }
}
