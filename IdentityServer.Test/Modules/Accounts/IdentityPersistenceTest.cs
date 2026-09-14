using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Http;
using IdentityServer.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using IdentityServer.Test.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.Accounts.Abstractions.Challenges;
using IdentityServer.Modules.Accounts.Abstractions.Email;
using IdentityServer.Modules.Accounts.Abstractions.Recovery;
using IdentityServer.Modules.Accounts.Abstractions.Security;
using IdentityServer.Modules.Accounts.Contracts.Challenges;
using IdentityServer.Modules.Accounts.Services.Challenges;
using IdentityServer.Modules.Accounts.Services.Email;
using IdentityServer.Modules.Accounts.Services.Recovery;
using IdentityServer.Modules.Accounts.Services.Security;
using IdentityServer.Modules.Accounts.Services.SignIn;
using IdentityServer.Modules.OpenIddict;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;
using AppIdentityRole = IdentityServer.Modules.Accounts.IdentityRole;
using AppIdentityUser = IdentityServer.Modules.Accounts.IdentityUser;
using IdentityServer.Modules.Accounts.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;

namespace IdentityServer.Test.Modules.Accounts
{
    public sealed class IdentityPersistenceTest
    {
        [Fact]
        public async Task UserManager_Should_Hash_Password_And_Enforce_Lockout()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
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
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
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
        public async Task AccountChallenge_Should_Lock_Account_After_Three_Invalid_Codes()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var challenges = services.GetRequiredService<IAccountChallengeService>();
            var user = CreateUser();
            Assert.True((await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword)).Succeeded);
            var delivery = await challenges.Create(
                user,
                AccountConstants.ChallengePurposes.PasswordRecovery,
                TimeSpan.FromMinutes(15),
                TestContext.Current.CancellationToken);
            Assert.NotNull(delivery);
            var invalidCode = delivery.Code == "000000" ? "000001" : "000000";

            var first = await challenges.Verify(
                delivery.LinkToken,
                invalidCode,
                AccountConstants.ChallengePurposes.PasswordRecovery,
                TestContext.Current.CancellationToken);
            var second = await challenges.Verify(
                delivery.LinkToken,
                invalidCode,
                AccountConstants.ChallengePurposes.PasswordRecovery,
                TestContext.Current.CancellationToken);
            var third = await challenges.Verify(
                delivery.LinkToken,
                invalidCode,
                AccountConstants.ChallengePurposes.PasswordRecovery,
                TestContext.Current.CancellationToken);

            Assert.False(first.LockedOut);
            Assert.False(second.LockedOut);
            Assert.True(third.LockedOut);
            Assert.True(user.RequiresAdminUnlock);
            Assert.Equal(DateTimeOffset.MaxValue, user.LockoutEnd);
        }

        [Fact]
        public async Task EmailConfirmation_Should_Invalidate_Challenge_Without_Locking_Account()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var challenges = services.GetRequiredService<IAccountChallengeService>();
            var user = CreateUser();
            Assert.True((await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword)).Succeeded);
            var delivery = await challenges.Create(
                user,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TimeSpan.FromMinutes(15),
                TestContext.Current.CancellationToken);
            Assert.NotNull(delivery);
            var invalidCode = delivery.Code == "000000" ? "000001" : "000000";

            await challenges.Verify(
                delivery.LinkToken,
                invalidCode,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TestContext.Current.CancellationToken);
            await challenges.Verify(
                delivery.LinkToken,
                invalidCode,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TestContext.Current.CancellationToken);
            var third = await challenges.Verify(
                delivery.LinkToken,
                invalidCode,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TestContext.Current.CancellationToken);

            Assert.False(third.LockedOut);
            Assert.False(user.RequiresAdminUnlock);
            Assert.Null(user.LockoutEnd);
        }

        [Fact]
        public async Task AccountChallenge_Should_Revoke_Older_Link_And_Consume_Valid_Link()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var challenges = services.GetRequiredService<IAccountChallengeService>();
            var database = services.GetRequiredService<IdentityServerDbContext>();
            var user = CreateUser();
            Assert.True((await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword)).Succeeded);
            var first = await challenges.Create(
                user,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TimeSpan.FromMinutes(15),
                TestContext.Current.CancellationToken);
            Assert.NotNull(first);
            var firstRecord = await database.AccountChallenges.SingleAsync(
                value => value.Id == first.ChallengeId,
                TestContext.Current.CancellationToken);
            firstRecord.CreatedUtc = DateTime.UtcNow.AddMinutes(-2);
            await database.SaveChangesAsync(TestContext.Current.CancellationToken);
            var current = await challenges.Create(
                user,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TimeSpan.FromMinutes(15),
                TestContext.Current.CancellationToken);
            Assert.NotNull(current);

            var revoked = await challenges.Verify(
                first.LinkToken,
                first.Code,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TestContext.Current.CancellationToken);
            var valid = await challenges.Verify(
                current.LinkToken,
                current.Code,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TestContext.Current.CancellationToken);

            Assert.False(revoked.Succeeded);
            Assert.True(valid.Succeeded);
            Assert.NotNull(valid.VerifiedChallenge);
            await challenges.Consume(valid.VerifiedChallenge!.Challenge, TestContext.Current.CancellationToken);
            Assert.All(await database.AccountChallenges.ToListAsync(TestContext.Current.CancellationToken), value =>
                Assert.NotNull(value.ConsumedUtc));
            Assert.DoesNotContain(current.Code, (await database.AccountChallenges.SingleAsync(
                value => value.Id == current.ChallengeId,
                TestContext.Current.CancellationToken)).CodeHash);
        }

        [Fact]
        public async Task AccountChallenge_Should_Enforce_Resend_Cooldown_And_Daily_Limit()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var challenges = services.GetRequiredService<IAccountChallengeService>();
            var database = services.GetRequiredService<IdentityServerDbContext>();
            var user = CreateUser();
            Assert.True((await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword)).Succeeded);

            var first = await challenges.Create(
                user,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TimeSpan.FromMinutes(15),
                TestContext.Current.CancellationToken);
            var duringCooldown = await challenges.Create(
                user,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TimeSpan.FromMinutes(15),
                TestContext.Current.CancellationToken);

            Assert.NotNull(first);
            Assert.Null(duringCooldown);

            for (var issuedCount = 1; issuedCount < 5; issuedCount++)
            {
                var latest = await database.AccountChallenges
                    .OrderByDescending(value => value.CreatedUtc)
                    .FirstAsync(TestContext.Current.CancellationToken);
                latest.CreatedUtc = DateTime.UtcNow.AddMinutes(-(issuedCount + 1));
                await database.SaveChangesAsync(TestContext.Current.CancellationToken);

                Assert.NotNull(await challenges.Create(
                    user,
                    AccountConstants.ChallengePurposes.EmailConfirmation,
                    TimeSpan.FromMinutes(15),
                    TestContext.Current.CancellationToken));
            }

            var last = await database.AccountChallenges
                .OrderByDescending(value => value.CreatedUtc)
                .FirstAsync(TestContext.Current.CancellationToken);
            last.CreatedUtc = DateTime.UtcNow.AddMinutes(-10);
            await database.SaveChangesAsync(TestContext.Current.CancellationToken);

            var overDailyLimit = await challenges.Create(
                user,
                AccountConstants.ChallengePurposes.EmailConfirmation,
                TimeSpan.FromMinutes(15),
                TestContext.Current.CancellationToken);

            Assert.Null(overDailyLimit);
            Assert.Equal(5, await database.AccountChallenges.CountAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Admin_Unlock_Should_Require_Key_And_Revoke_Active_Challenges()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<AppIdentityRole>>();
            var challenges = services.GetRequiredService<IAccountChallengeService>();
            var database = services.GetRequiredService<IdentityServerDbContext>();
            var securityEvents = services.GetRequiredService<IIdentitySecurityEventRecorder>();
            var timeProvider = services.GetRequiredService<TimeProvider>();
            var user = CreateUser();
            user.LockoutEnd = DateTimeOffset.MaxValue;
            user.RequiresAdminUnlock = true;
            user.AdminLockoutUtc = DateTime.UtcNow;
            user.AccessFailedCount = 3;
            Assert.True((await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword)).Succeeded);
            await challenges.Create(
                user,
                AccountConstants.ChallengePurposes.PasswordRecovery,
                TimeSpan.FromMinutes(15),
                TestContext.Current.CancellationToken);
            var adminOptions = Options.Create(new IdentityAdminApiOptions
            {
                Enabled = true,
                ApiKeyHeaderName = "X-Test-Admin-Key",
                ApiKey = "expected-key"
            });
            var controller = new IdentityAdminController(
                userManager,
                roleManager,
                database,
                securityEvents,
                timeProvider,
                adminOptions)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            var unauthorized = await controller.UnlockUser(user.Id, TestContext.Current.CancellationToken);
            controller.Request.Headers[adminOptions.Value.ApiKeyHeaderName] = adminOptions.Value.ApiKey;
            var unlocked = await controller.UnlockUser(user.Id, TestContext.Current.CancellationToken);

            Assert.IsType<UnauthorizedResult>(unauthorized);
            Assert.IsType<NoContentResult>(unlocked);
            Assert.Null(user.LockoutEnd);
            Assert.False(user.RequiresAdminUnlock);
            Assert.Null(user.AdminLockoutUtc);
            Assert.Equal(0, user.AccessFailedCount);
            Assert.All(await database.AccountChallenges.ToListAsync(TestContext.Current.CancellationToken), value =>
                Assert.NotNull(value.ConsumedUtc));
        }

        [Fact]
        public async Task PasswordRecovery_Should_Activate_Reset_And_Consume_Grant()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var recovery = services.GetRequiredService<IAccountRecoveryService>();
            var email = services.GetRequiredService<CapturingAccountEmailService>();
            var user = CreateUser();
            user.EmailConfirmed = true;
            Assert.True((await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword)).Succeeded);

            await recovery.RequestPasswordRecovery(
                user.Email!,
                AccountConstants.Cultures.English,
                TestContext.Current.CancellationToken);
            var verified = await recovery.VerifyPasswordRecovery(
                email.PasswordRecoveryChallenge!.LinkToken,
                email.PasswordRecoveryChallenge.Code,
                TestContext.Current.CancellationToken);
            var repeatedVerification = await recovery.VerifyPasswordRecovery(
                email.PasswordRecoveryChallenge.LinkToken,
                email.PasswordRecoveryChallenge.Code,
                TestContext.Current.CancellationToken);
            var newPassword = "Different-password-456!";
            var reset = await recovery.ResetPassword(
                verified.ResetGrant!,
                newPassword,
                TestContext.Current.CancellationToken);
            var repeated = await recovery.ResetPassword(
                verified.ResetGrant!,
                "Another-password-789!",
                TestContext.Current.CancellationToken);

            Assert.True(verified.Succeeded);
            Assert.False(repeatedVerification.Succeeded);
            Assert.True(reset.Result.Succeeded);
            Assert.False(repeated.Result.Succeeded);
            Assert.True(await userManager.CheckPasswordAsync(user, newPassword));
        }

        [Fact]
        public async Task PasswordRecovery_Should_Permanently_Lock_Account_After_Three_Rejected_Passwords()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var recovery = services.GetRequiredService<IAccountRecoveryService>();
            var email = services.GetRequiredService<CapturingAccountEmailService>();
            var user = CreateUser();
            user.EmailConfirmed = true;
            Assert.True((await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword)).Succeeded);

            await recovery.RequestPasswordRecovery(
                user.Email!,
                AccountConstants.Cultures.English,
                TestContext.Current.CancellationToken);
            var verified = await recovery.VerifyPasswordRecovery(
                email.PasswordRecoveryChallenge!.LinkToken,
                email.PasswordRecoveryChallenge.Code,
                TestContext.Current.CancellationToken);

            var first = await recovery.ResetPassword(
                verified.ResetGrant!,
                "weak",
                TestContext.Current.CancellationToken);
            var second = await recovery.ResetPassword(
                verified.ResetGrant!,
                "weak",
                TestContext.Current.CancellationToken);
            var third = await recovery.ResetPassword(
                verified.ResetGrant!,
                "weak",
                TestContext.Current.CancellationToken);

            Assert.False(first.LockedOut);
            Assert.False(second.LockedOut);
            Assert.True(third.LockedOut);
            Assert.True(user.RequiresAdminUnlock);
            Assert.Equal(DateTimeOffset.MaxValue, user.LockoutEnd);
        }

        [Fact]
        public async Task EmailConfirmation_Should_Consume_Link_And_Reject_Replay()
        {
            await using var services = CreateServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var recovery = services.GetRequiredService<IAccountRecoveryService>();
            var email = services.GetRequiredService<CapturingAccountEmailService>();
            var user = CreateUser();
            Assert.True((await userManager.CreateAsync(user, IdentityServerTestConstants.TestPassword)).Succeeded);

            await recovery.QueueEmailConfirmation(
                user,
                AccountConstants.Cultures.English,
                TestContext.Current.CancellationToken);
            var confirmed = await recovery.ConfirmEmail(
                email.ConfirmationChallenge!.LinkToken,
                email.ConfirmationChallenge.Code,
                TestContext.Current.CancellationToken);
            var replayed = await recovery.ConfirmEmail(
                email.ConfirmationChallenge.LinkToken,
                email.ConfirmationChallenge.Code,
                TestContext.Current.CancellationToken);

            Assert.True(confirmed.Succeeded);
            Assert.False(replayed.Succeeded);
            Assert.True(user.EmailConfirmed);
        }

        [Fact]
        public async Task Configured_Identity_Principal_Should_Use_OpenIddict_Claim_Types()
        {
            await using var services = CreateConfiguredIdentityServices();
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var principalFactory = services.GetRequiredService<IUserClaimsPrincipalFactory<AppIdentityUser>>();
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
            var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<AppIdentityRole>>();
            Assert.True((await roleManager.CreateAsync(new AppIdentityRole
            {
                Id = Guid.NewGuid(),
                Name = AccountConstants.DefaultRole
            })).Succeeded);
            Assert.True((await roleManager.CreateAsync(new AppIdentityRole
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
            Assert.False(context.Response.Headers.ContainsKey(HeaderNames.Location));
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
            services.AddIdentityCore<AppIdentityUser>(options =>
                {
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                })
                .AddRoles<AppIdentityRole>()
                .AddEntityFrameworkStores<TestIdentityServerDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<IIdentitySecurityEventRecorder, IdentitySecurityEventRecorder>();
            services.Configure<AccountEmailOptions>(options =>
            {
                options.VerificationMaxFailedAttempts = 3;
                options.PasswordResetMaxFailedAttempts = 3;
                options.ResendCooldownSeconds = 60;
                options.DailyDeliveryLimit = 5;
                options.DispatchPollSeconds = 5;
                options.DispatchBatchSize = 10;
                options.DispatchRetryBaseSeconds = 15;
                options.DispatchRetryMaxMinutes = 60;
                options.CleanupIntervalMinutes = 60;
                options.ChallengeRetentionDays = 30;
                options.SecurityEventRetentionDays = 90;
                options.ConfirmationLifetimeMinutes = 1440;
                options.PasswordRecoveryLifetimeMinutes = 15;
                options.ResetGrantLifetimeMinutes = 15;
            });
            services.AddScoped<IAccountChallengeService, AccountChallengeService>();
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.AddSingleton<CapturingAccountEmailService>();
            services.AddSingleton<IAccountEmailService>(serviceProvider =>
                serviceProvider.GetRequiredService<CapturingAccountEmailService>());
            services.AddScoped<IAccountRecoveryService, AccountRecoveryService>();

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
                    ["AccountEmail:ApplicationBaseUrl"] = "http://localhost:4200",
                    ["AccountEmail:ConfirmationLifetimeMinutes"] = "1440",
                    ["AccountEmail:PasswordRecoveryLifetimeMinutes"] = "15",
                    ["AccountEmail:ResetGrantLifetimeMinutes"] = "15",
                    ["AccountEmail:VerificationMaxFailedAttempts"] = "3",
                    ["AccountEmail:PasswordResetMaxFailedAttempts"] = "3",
                    ["AccountEmail:ResendCooldownSeconds"] = "60",
                    ["AccountEmail:DailyDeliveryLimit"] = "5",
                    ["AccountEmail:DispatchPollSeconds"] = "5",
                    ["AccountEmail:DispatchBatchSize"] = "10",
                    ["AccountEmail:DispatchRetryBaseSeconds"] = "15",
                    ["AccountEmail:DispatchRetryMaxMinutes"] = "60",
                    ["AccountEmail:CleanupIntervalMinutes"] = "60",
                    ["AccountEmail:ChallengeRetentionDays"] = "30",
                    ["AccountEmail:SecurityEventRetentionDays"] = "90"
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

        private static AppIdentityUser CreateUser()
        {
            var now = TimeProvider.System.GetUtcNow().UtcDateTime;
            var id = Guid.NewGuid();

            return new AppIdentityUser
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

        private sealed class CapturingAccountEmailService : IAccountEmailService
        {
            public AccountChallengeDelivery? ConfirmationChallenge { get; private set; }
            public AccountChallengeDelivery? PasswordRecoveryChallenge { get; private set; }

            public Task QueueConfirmation(
                AppIdentityUser user,
                AccountChallengeDelivery challenge,
                string? culture,
                CancellationToken cancellationToken)
            {
                ConfirmationChallenge = challenge;
                return Task.CompletedTask;
            }

            public Task QueuePasswordRecovery(
                AppIdentityUser user,
                AccountChallengeDelivery challenge,
                string? culture,
                CancellationToken cancellationToken)
            {
                PasswordRecoveryChallenge = challenge;
                return Task.CompletedTask;
            }
        }
    }
}
