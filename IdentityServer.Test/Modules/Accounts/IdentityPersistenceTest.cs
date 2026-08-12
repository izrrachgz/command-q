using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using IdentityServer.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IdentityServer.Modules.Accounts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            const string password = "Correct-Horse-42!";

            var created = await userManager.CreateAsync(user, password);

            Assert.True(created.Succeeded);
            Assert.NotNull(user.PasswordHash);
            Assert.NotEqual(password, user.PasswordHash);
            Assert.True(await userManager.CheckPasswordAsync(user, password));

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
                new UserLoginInfo("Google", "provider-subject", "Google"))).Succeeded);

            var found = await userManager.FindByLoginAsync("Google", "provider-subject");

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

        private static ServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            services.AddLogging();
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
                    ["Identity:CookieName"] = "CommandQ.Identity.Test",
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
    }
}
