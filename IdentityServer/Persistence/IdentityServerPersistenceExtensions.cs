using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IdentityServer.Localization;
using IdentityServer.Resources;
using IdentityServer.Persistence.Providers;

namespace IdentityServer.Persistence
{
    internal static class IdentityServerPersistenceExtensions
    {
        public static IServiceCollection AddIdentityServerPersistence(
            this IServiceCollection services,
            IConfiguration configuration,
            IdentityBuilder identityBuilder)
        {
            var settings = configuration.GetIdentityDatabaseOptions();

            switch (settings.Engine)
            {
                case IdentityDatabaseEngine.PgSql:
                    AddProvider<PgSqlIdentityServerDbContext>(
                        services,
                        identityBuilder,
                        options => options.UseNpgsql(
                            settings.ConnectionString,
                            provider => provider.EnableRetryOnFailure()));
                    break;
                case IdentityDatabaseEngine.MsSql:
                    AddProvider<MsSqlIdentityServerDbContext>(
                        services,
                        identityBuilder,
                        options => options.UseSqlServer(
                            settings.ConnectionString,
                            provider => provider.EnableRetryOnFailure()));
                    break;
                case IdentityDatabaseEngine.MySql:
                    AddProvider<MySqlIdentityServerDbContext>(
                        services,
                        identityBuilder,
                        options => options.UseMySQL(
                            settings.ConnectionString,
                            provider => provider.EnableRetryOnFailure()));
                    break;
                case IdentityDatabaseEngine.SqLite:
                    AddProvider<SqLiteIdentityServerDbContext>(
                        services,
                        identityBuilder,
                        options => options.UseSqlite(settings.ConnectionString));
                    break;
                default:
                    throw new InvalidOperationException(
                        IdentityServerText.Format(
                            IdentityServerMessages.Configuration_UnsupportedIdentityServerDatabaseEngine,
                            settings.Engine));
            }

            return services;
        }

        private static void AddProvider<TContext>(
            IServiceCollection services,
            IdentityBuilder identityBuilder,
            Action<DbContextOptionsBuilder> configureProvider)
            where TContext : IdentityServerDbContext
        {
            services.AddDbContext<TContext>(options =>
            {
                configureProvider(options);
                options.UseOpenIddict();
            });

            services.AddScoped<IdentityServerDbContext>(serviceProvider =>
                serviceProvider.GetRequiredService<TContext>());

            identityBuilder.AddEntityFrameworkStores<TContext>();

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<TContext>();
                });
        }
    }
}
