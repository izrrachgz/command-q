using FluentValidation;
using Common.Models.Response;
using Domain.Modules.Identity;
using Application.Abstractions;
using Application.Extensions;
using Application.Modules.Identity.Roles.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Application.Modules.Identity.Accounts.Dtos;
using Application.Modules.Identity.Roles.Queries;
using Application.Modules.Identity.Roles.Commands;
using Application.Modules.Identity.Roles.Services;
using Application.Modules.Identity.Accounts.Queries;
using Application.Modules.Identity.Accounts.Commands;
using Application.Modules.Identity.Accounts.Mappings;
using Application.Modules.Identity.Accounts.Services;
using Application.Modules.Identity.AccountActivity.Dtos;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Application.Modules.Identity.AccountActivity.Queries;
using Application.Modules.Identity.AccountActivity.Commands;
using Application.Modules.Identity.AccountActivity.Mappings;
using Application.Modules.Identity.AccountActivity.Services;
using Application.Modules.Identity.Accounts.Preferences.Dtos;
using Application.Modules.Identity.Accounts.Preferences.Queries;
using Application.Modules.Identity.Accounts.Preferences.Commands;
using Application.Modules.Identity.Accounts.Preferences.Mappings;
using Application.Modules.Identity.Accounts.Preferences.Services;

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
            services.AddScoped<AccountPreferenceService>();
            services.AddScoped<IAccountActivityRecorder, AccountActivityRecorder>();
            services.TryAddScoped<IIdentityRoleAdministrationClient, UnavailableIdentityRoleAdministrationClient>();

            services.AddApplicationRequest<CreateAccountCommand, BasicResponse, CreateAccountCommandHandler>();
            services.AddApplicationRequest<GetAccountByIdentityQuery, DetailResponse<AccountModel>, GetAccountByIdentityQueryHandler>();
            services.AddApplicationRequest<GetAccountByIdQuery, DetailResponse<AccountModel>, GetAccountByIdQueryHandler>();
            services.AddApplicationRequest<GetAccountByUsernameQuery, DetailResponse<AccountModel>, GetAccountByUsernameQueryHandler>();
            services.AddApplicationRequest<RecordAccountActivityCommand, BasicResponse, RecordAccountActivityCommandHandler>();
            services.AddApplicationRequest<GetAccountActivityQuery, PagedResponse<AccountActivityModel>, GetAccountActivityQueryHandler>();
            services.AddApplicationRequest<SearchIdentityUsersQuery, PagedResponse<IdentityUserRoleModel>, SearchIdentityUsersQueryHandler>();
            services.AddApplicationRequest<
                GetIdentityUserRolesQuery,
                DetailResponse<IdentityUserRoleModel>,
                GetIdentityUserRolesQueryHandler>();
            services.AddApplicationRequest<GetIdentityRolesQuery, DetailResponse<IdentityRoleListModel>, GetIdentityRolesQueryHandler>();
            services.AddApplicationRequest<UpdateIdentityUserRolesCommand, BasicResponse, UpdateIdentityUserRolesCommandHandler>();
            services.AddApplicationRequest<UnlockIdentityUserCommand, BasicResponse, UnlockIdentityUserCommandHandler>();
            services.AddApplicationRequest<SaveAccountPreferencesCommand, BasicResponse, SaveAccountPreferencesCommandHandler>();
            services.AddApplicationRequest<
                GetAccountPreferencesQuery,
                DetailResponse<AccountPreferenceModel>,
                GetAccountPreferencesQueryHandler>();

            services.AddScoped<IValidator<CreateAccountCommand>, CreateAccountCommandValidator>();
            services.AddScoped<IValidator<GetAccountByIdentityQuery>, GetAccountByIdentityQueryValidator>();
            services.AddScoped<IValidator<GetAccountByIdQuery>, GetAccountByIdQueryValidator>();
            services.AddScoped<IValidator<GetAccountByUsernameQuery>, GetAccountByUsernameQueryValidator>();
            services.AddScoped<IValidator<RecordAccountActivityCommand>, RecordAccountActivityCommandValidator>();
            services.AddScoped<IValidator<GetAccountActivityQuery>, GetAccountActivityQueryValidator>();
            services.AddScoped<IValidator<SearchIdentityUsersQuery>, SearchIdentityUsersQueryValidator>();
            services.AddScoped<IValidator<GetIdentityUserRolesQuery>, GetIdentityUserRolesQueryValidator>();
            services.AddScoped<IValidator<UpdateIdentityUserRolesCommand>, UpdateIdentityUserRolesCommandValidator>();
            services.AddScoped<IValidator<UnlockIdentityUserCommand>, UnlockIdentityUserCommandValidator>();
            services.AddScoped<IValidator<SaveAccountPreferencesCommand>, SaveAccountPreferencesCommandValidator>();
            services.AddScoped<IValidator<GetAccountPreferencesQuery>, GetAccountPreferencesQueryValidator>();

            services.AddSingleton<IMapper<Account, AccountModel>, AccountMapper>();
            services.AddSingleton<IMapper<Domain.Modules.Identity.AccountActivity, AccountActivityModel>, AccountActivityMapper>();
            services.AddSingleton<IMapper<AccountPreference, AccountPreferenceModel>, AccountPreferenceMapper>();

            return services;
        }
    }
}
