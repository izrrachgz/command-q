using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Http;
using IdentityServer.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using IdentityServer.Test.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.OpenIddict;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using CommandQIdentityRole = IdentityServer.Modules.Accounts.IdentityRole;
using CommandQIdentityUser = IdentityServer.Modules.Accounts.IdentityUser;

namespace IdentityServer.Test.Modules.Accounts
{
    public sealed class IdentityPersistenceTest
    {
        [Fact]
        public async Task UserManager_Should_Hash_Password_And_Enforce_Lockout()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<CommandQIdentityUser>>();
            var user = CreateUser();

            var created = await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword);

            Assert.True(created.Succeeded);
            Assert.NotNull(user.PasswordHash);
            Assert.NotEqual(IdentityServerTestConstants.TestPassword, user.PasswordHash);
            Assert.True(await userManager.CheckPasswordAsync(user, IdentityServerTestConstants.TestPassword));

            for (var attempt = 0; attempt < 3; attempt++)
            {
                await userManager.AccessFailedAsync(user);
            }

            Assert.True(await userManager.IsLockedOutAsync(user));
        }

        [Fact]
        public async Task UserManager_Should_Persist_External_Login_Link()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<CommandQIdentityUser>>();
            var user = CreateUser();

            Assert.True((await userManager.CreateAsync(user)).Succeeded);
            Assert.True((await userManager.AddLoginAsync(
                user,
                new UserLoginInfo(
                    IdentityServerTestConstants.ExternalLoginProviderName,
                    IdentityServerTestConstants.ExternalLoginProviderKey,
                    IdentityServerTestConstants.ExternalLoginProviderName))).Succeeded);

            var found = await userManager.FindByLoginAsync(
                IdentityServerTestConstants.ExternalLoginProviderName,
                IdentityServerTestConstants.ExternalLoginProviderKey);

            Assert.Equal(user.Id, found?.Id);
        }

        [Fact]
        public async Task SecurityEventRecorder_Should_Persist_Queryable_Event()
        {
            await using var services = CreateServices();
            var recorder = services.GetRequiredService<IIdentitySecurityEventRecorder>();
            var database = services.GetRequiredService<IdentityServerDbContext>();
            var userId = Guid.NewGuid();

            await recorder.Record(
                AccountConstants.Actions.LoginSucceeded,
                userId,
                cancellationToken: TestContext.Current.CancellationToken);

            var securityEvent = await database.SecurityEvents.SingleAsync(TestContext.Current.CancellationToken);
            Assert.Equal(userId, securityEvent.UserId);
            Assert.Equal(AccountConstants.Actions.LoginSucceeded, securityEvent.Action);
        }

        [Fact]
        public async Task Configured_Identity_Principal_Should_Use_OpenIddict_Claim_Types()
        {
            await using var services = CreateConfiguredIdentityServices();
            var userManager = services.GetRequiredService<UserManager<CommandQIdentityUser>>();
            var principalFactory = services.GetRequiredService<IUserClaimsPrincipalFactory<CommandQIdentityUser>>();
            var user = CreateUser();

            Assert.True((await userManager.CreateAsync(user)).Succeeded);

            var principal = await principalFactory.CreateAsync(user);
            var resolvedUser = await userManager.GetUserAsync(principal);

            Assert.Equal(user.Id.ToString(), principal.FindFirst(OpenIddictConstants.Claims.Subject)?.Value);
            Assert.Equal(user.UserName, principal.FindFirst(OpenIddictConstants.Claims.Name)?.Value);
            Assert.Equal(user.Email, principal.FindFirst(OpenIddictConstants.Claims.Email)?.Value);
            Assert.Equal(user.Id, resolvedUser?.Id);
        }

        [Fact]
        public async Task Development_First_User_Should_Receive_Admin_Without_Restart()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<CommandQIdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<CommandQIdentityRole>>();
            Assert.True((await roleManager.CreateAsync(new CommandQIdentityRole
            {
                Id = Guid.NewGuid(),
                Name = AccountConstants.DefaultRole
            })).Succeeded);
            Assert.True((await roleManager.CreateAsync(new CommandQIdentityRole
            {
                Id = Guid.NewGuid(),
                Name = AccountConstants.AdminRole
            })).Succeeded);
            var user = CreateUser();
            Assert.True((await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword)).Succeeded);
            Assert.True((await userManager.AddToRoleAsync(user, AccountConstants.DefaultRole)).Succeeded);
            var assigner = new DevelopmentAdminRoleAssigner(userManager, new DevelopmentEnvironment());

            var result = await assigner.AssignIfRequired(user);

            Assert.True(result.Succeeded);
            Assert.True(await userManager.IsInRoleAsync(user, AccountConstants.AdminRole));
        }

        [Fact]
        public void Configured_Application_Cookie_Should_Support_First_Party_Identity_Sign_In()
        {
            using var services = CreateConfiguredIdentityServices();
            var options = services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(IdentityConstants.ApplicationScheme);

            Assert.Equal(IdentityServerTestConstants.TestCookieName, options.Cookie.Name);
            Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
            Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
            Assert.Equal(AccountConstants.Routes.SignInPath, options.LoginPath);
            Assert.Equal(AccountConstants.ReturnUrlParameter, options.ReturnUrlParameter);
            Assert.True(options.Cookie.HttpOnly);
        }

        [Fact]
        public async Task Configured_Application_Cookie_Should_Redirect_Browser_Authorization_Challenges()
        {
            using var services = CreateConfiguredIdentityServices();
            var options = services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(IdentityConstants.ApplicationScheme);
            var context = CreateRedirectContext(
                options,
                OpenIddictRoutes.Authorization,
                IdentityServerTestConstants.BrowserAccept);

            await options.Events.RedirectToLogin(context);

            Assert.Equal(StatusCodes.Status302Found, context.Response.StatusCode);
            Assert.Equal(IdentityServerTestConstants.BrowserAuthorizationRedirectLocation, context.Response.Headers.Location);
        }

        [Fact]
        public async Task Configured_Application_Cookie_Should_Return_Unauthorized_For_Api_Challenges()
        {
            using var services = CreateConfiguredIdentityServices();
            var options = services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(IdentityConstants.ApplicationScheme);
            var context = CreateRedirectContext(
                options,
                AccountConstants.Routes.MePath,
                IdentityServerTestConstants.JsonAccept);

            await options.Events.RedirectToLogin(context);

            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
            Assert.False(context.Response.Headers.ContainsKey("Location"));
        }

        private static ServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            services.AddLogging();
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
            services.AddDbContext<TestIdentityServerDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                options.UseOpenIddict();
            });
            services.AddScoped<IdentityServerDbContext>(serviceProvider =>
                serviceProvider.GetRequiredService<TestIdentityServerDbContext>());
            services.AddIdentityCore<CommandQIdentityUser>(options =>
                {
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                })
                .AddRoles<CommandQIdentityRole>()
                .AddEntityFrameworkStores<TestIdentityServerDbContext>();
            services.AddScoped<IIdentitySecurityEventRecorder, IdentitySecurityEventRecorder>();

            return services.BuildServiceProvider();
        }

        private static ServiceProvider CreateConfiguredIdentityServices()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Identity:RequiredPasswordLength"] = "12",
                    ["Identity:RequireDigit"] = "true",
                    ["Identity:RequireLowercase"] = "true",
                    ["Identity:RequireNonAlphanumeric"] = "true",
                    ["Identity:RequireUppercase"] = "true",
                    ["Identity:MaxFailedAccessAttempts"] = "5",
                    ["Identity:LockoutMinutes"] = "15",
                    ["Identity:RequireConfirmedEmail"] = "false",
                    ["Identity:CookieHours"] = "8",
                    ["Identity:CookieName"] = IdentityServerTestConstants.TestCookieName,
                    ["Identity:AuthenticationPermitLimit"] = "5",
                    ["Identity:AuthenticationWindowSeconds"] = "60",
                    ["Identity:AuthenticationQueueLimit"] = "0"
                })
                .Build();
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddDbContext<TestIdentityServerDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                options.UseOpenIddict();
            });
            services.AddScoped<IdentityServerDbContext>(serviceProvider =>
                serviceProvider.GetRequiredService<TestIdentityServerDbContext>());
            services.AddAccountsModule(configuration)
                .AddEntityFrameworkStores<TestIdentityServerDbContext>();

            return services.BuildServiceProvider();
        }

        private static RedirectContext<CookieAuthenticationOptions> CreateRedirectContext(
            CookieAuthenticationOptions options,
            string requestPath,
            string accept)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.Path = requestPath;
            httpContext.Request.Headers.Accept = accept;

            return new RedirectContext<CookieAuthenticationOptions>(
                httpContext,
                new AuthenticationScheme(IdentityConstants.ApplicationScheme, displayName: null, typeof(CookieAuthenticationHandler)),
                options,
                new AuthenticationProperties(),
                IdentityServerTestConstants.BrowserAuthorizationRedirectLocation);
        }

        private static CommandQIdentityUser CreateUser()
        {
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid();

            return new CommandQIdentityUser
            {
                Id = id,
                UserName = $"user-{id:N}",
                Email = $"user-{id:N}@example.com",
                CreatedUtc = now,
                ModifiedUtc = now
            };
        }

        private sealed class TestIdentityServerDbContext(DbContextOptions<TestIdentityServerDbContext> options)
            : IdentityServerDbContext(options)
        {
        }

        private sealed class DevelopmentEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; } = Environments.Development;
            public string ApplicationName { get; set; } = "IdentityServer.Test";
            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }
    }
}
