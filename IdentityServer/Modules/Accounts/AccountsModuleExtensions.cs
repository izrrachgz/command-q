using OpenIddict.Abstractions;
using IdentityServer.Constants;
using IdentityServer.Resources;
using IdentityServer.Localization;
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using IdentityServer.Modules.Accounts.Configuration;

namespace IdentityServer.Modules.Accounts
{
    internal static class AccountsModuleExtensions
    {
        public static IdentityBuilder AddAccountsModule(this IServiceCollection services, IConfiguration configuration)
        {
            var identitySection = configuration.GetRequiredSection(IdentityServerConfigKeys.Identity.Section);

            services.AddOptions<IdentityConfigurationOptions>()
                .Bind(identitySection)
                .Validate(
                    options => options.RequiredPasswordLength >= 8,
                    IdentityServerText.Format(
                        IdentityServerMessages.Configuration_MinimumValue,
                        IdentityPath(IdentityServerConfigKeys.Identity.RequiredPasswordLength),
                        8))
                .Validate(
                    options => options.MaxFailedAccessAttempts > 0,
                    GreaterThanZero(IdentityServerConfigKeys.Identity.MaxFailedAccessAttempts))
                .Validate(
                    options => options.LockoutMinutes > 0,
                    GreaterThanZero(IdentityServerConfigKeys.Identity.LockoutMinutes))
                .Validate(
                    options => options.CookieHours > 0,
                    GreaterThanZero(IdentityServerConfigKeys.Identity.CookieHours))
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.CookieName),
                    Required(IdentityServerConfigKeys.Identity.CookieName))
                .Validate(
                    options => options.AuthenticationPermitLimit > 0,
                    GreaterThanZero(IdentityServerConfigKeys.Identity.AuthenticationPermitLimit))
                .Validate(
                    options => options.AuthenticationWindowSeconds > 0,
                    GreaterThanZero(IdentityServerConfigKeys.Identity.AuthenticationWindowSeconds))
                .Validate(
                    options => options.AuthenticationQueueLimit >= 0,
                    IdentityServerText.Format(
                        IdentityServerMessages.Configuration_CannotBeNegative,
                        IdentityPath(IdentityServerConfigKeys.Identity.AuthenticationQueueLimit)))
                .ValidateOnStart();

            var identityOptions = identitySection.Get<IdentityConfigurationOptions>()
                ?? throw new InvalidOperationException(
                    IdentityServerMessages.Configuration_IdentityRequired);

            var identityBuilder = services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                    options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                    options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
                    options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
                    options.Password.RequiredLength = identityOptions.RequiredPasswordLength;
                    options.Password.RequireDigit = identityOptions.RequireDigit;
                    options.Password.RequireLowercase = identityOptions.RequireLowercase;
                    options.Password.RequireNonAlphanumeric = identityOptions.RequireNonAlphanumeric;
                    options.Password.RequireUppercase = identityOptions.RequireUppercase;
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = identityOptions.MaxFailedAccessAttempts;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityOptions.LockoutMinutes);
                    options.SignIn.RequireConfirmedEmail = identityOptions.RequireConfirmedEmail;
                    options.User.RequireUniqueEmail = true;
                })
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = identityOptions.CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = AccountConstants.Routes.SignInPath;
                options.ReturnUrlParameter = AccountConstants.ReturnUrlParameter;
                options.ExpireTimeSpan = TimeSpan.FromHours(identityOptions.CookieHours);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    return RedirectOrStatusCode(context, StatusCodes.Status401Unauthorized);
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    return RedirectOrStatusCode(context, StatusCodes.Status403Forbidden);
                };
            });

            services.AddOptions<ExternalProviderOptions>()
                .Bind(configuration.GetSection(IdentityServerConfigKeys.ExternalProviders.Section))
                .Validate(
                    options => HasCompleteCredentials(options.Google),
                    IdentityServerText.Format(
                        IdentityServerMessages.Configuration_ExternalProviderCredentialsRequired,
                        "Google"))
                .Validate(
                    options => HasCompleteCredentials(options.Microsoft),
                    IdentityServerText.Format(
                        IdentityServerMessages.Configuration_ExternalProviderCredentialsRequired,
                        "Microsoft"))
                .ValidateOnStart();

            services.AddOptions<IdentityAdminApiOptions>()
                .Bind(configuration.GetSection(IdentityServerConfigKeys.AdminApi.Section))
                .Validate(
                    options => !options.Enabled || !string.IsNullOrWhiteSpace(options.ApiKeyHeaderName),
                    Required(IdentityServerConfigKeys.AdminApi.ApiKeyHeaderName))
                .Validate(
                    options => !options.Enabled || !string.IsNullOrWhiteSpace(options.ApiKey),
                    Required(IdentityServerConfigKeys.AdminApi.ApiKey))
                .ValidateOnStart();

            AddExternalProviders(services, configuration);

            services.AddHttpContextAccessor();
            services.AddSingleton<ISignInPageRenderer, SignInPageRenderer>();
            services.AddScoped<IIdentitySecurityEventRecorder, IdentitySecurityEventRecorder>();
            services.AddScoped<DevelopmentAdminRoleAssigner>();
            services.AddRateLimiter(options =>
            {
                options.AddPolicy(AccountConstants.AuthenticationRateLimitPolicy, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = identityOptions.AuthenticationPermitLimit,
                            Window = TimeSpan.FromSeconds(identityOptions.AuthenticationWindowSeconds),
                            QueueLimit = identityOptions.AuthenticationQueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            AutoReplenishment = true
                        }));
            });

            return identityBuilder;
        }

        private static void AddExternalProviders(IServiceCollection services, IConfiguration configuration)
        {
            var providers = configuration.GetSection(IdentityServerConfigKeys.ExternalProviders.Section).Get<ExternalProviderOptions>() ?? new();
            var authentication = services.AddAuthentication();

            if (IsConfigured(providers.Google))
            {
                authentication.AddGoogle(options =>
                {
                    options.ClientId = providers.Google.ClientId;
                    options.ClientSecret = providers.Google.ClientSecret;
                    options.SaveTokens = false;
                });
            }

            if (IsConfigured(providers.Microsoft))
            {
                authentication.AddMicrosoftAccount(options =>
                {
                    options.ClientId = providers.Microsoft.ClientId;
                    options.ClientSecret = providers.Microsoft.ClientSecret;
                    options.SaveTokens = false;
                });
            }
        }

        private static bool HasCompleteCredentials(ExternalProviderCredentials credentials)
        {
            return !IsConfigured(credentials) ||
                (!string.IsNullOrWhiteSpace(credentials.ClientId) && !string.IsNullOrWhiteSpace(credentials.ClientSecret));
        }

        private static bool IsConfigured(ExternalProviderCredentials credentials)
        {
            return !string.IsNullOrWhiteSpace(credentials.ClientId) || !string.IsNullOrWhiteSpace(credentials.ClientSecret);
        }

        private static Task RedirectOrStatusCode(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
        {
            if (ExpectsApiResponse(context.Request))
            {
                context.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }

        private static bool ExpectsApiResponse(HttpRequest request)
        {
            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            {
                return true;
            }

            if (string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return request.Headers.Accept.Any(value =>
                value?.Contains(AccountConstants.ContentTypes.Json, StringComparison.OrdinalIgnoreCase) == true);
        }

        private static string IdentityPath(string childPath)
        {
            return IdentityServerConfigKeys.FullPath(childPath);
        }

        private static string GreaterThanZero(string childPath)
        {
            return IdentityServerText.Format(IdentityServerMessages.Configuration_GreaterThanZero, IdentityPath(childPath));
        }

        private static string Required(string childPath)
        {
            return IdentityServerText.Format(IdentityServerMessages.Configuration_Required, IdentityPath(childPath));
        }
    }
}
