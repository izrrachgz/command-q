using IdentityServer.Constants;
using OpenIddict.Abstractions;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.Accounts
{
    internal static class AccountsModuleExtensions
    {
        public static IdentityBuilder AddAccountsModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var identitySection = configuration.GetRequiredSection(ConfigKeys.Identity.Section);

            services.AddOptions<IdentityConfigurationOptions>()
                .Bind(identitySection)
                .Validate(options => options.RequiredPasswordLength >= 8, "Identity:RequiredPasswordLength must be at least eight.")
                .Validate(options => options.MaxFailedAccessAttempts > 0, "Identity:MaxFailedAccessAttempts must be greater than zero.")
                .Validate(options => options.LockoutMinutes > 0, "Identity:LockoutMinutes must be greater than zero.")
                .Validate(options => options.CookieHours > 0, "Identity:CookieHours must be greater than zero.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.CookieName), "Identity:CookieName is required.")
                .Validate(options => options.AuthenticationPermitLimit > 0, "Identity:AuthenticationPermitLimit must be greater than zero.")
                .Validate(options => options.AuthenticationWindowSeconds > 0, "Identity:AuthenticationWindowSeconds must be greater than zero.")
                .Validate(options => options.AuthenticationQueueLimit >= 0, "Identity:AuthenticationQueueLimit cannot be negative.")
                .ValidateOnStart();

            var identityOptions = identitySection.Get<IdentityConfigurationOptions>()
                ?? throw new InvalidOperationException("Identity configuration is required.");

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
                options.ExpireTimeSpan = TimeSpan.FromHours(identityOptions.CookieHours);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

            services.AddOptions<ExternalProviderOptions>()
                .Bind(configuration.GetSection(ConfigKeys.ExternalProviders.Section))
                .Validate(options => HasCompleteCredentials(options.Google), "Google external provider requires both ClientId and ClientSecret.")
                .Validate(options => HasCompleteCredentials(options.Microsoft), "Microsoft external provider requires both ClientId and ClientSecret.")
                .ValidateOnStart();

            AddExternalProviders(services, configuration);

            services.AddHttpContextAccessor();
            services.AddScoped<IdentitySecurityEventRecorder>();
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
            var providers = configuration.GetSection(ConfigKeys.ExternalProviders.Section).Get<ExternalProviderOptions>() ?? new();
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
    }
}
