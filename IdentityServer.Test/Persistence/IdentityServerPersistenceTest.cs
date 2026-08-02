using IdentityServer.Modules.Accounts;
using IdentityServer.Persistence;
using IdentityServer.Persistence.Providers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommandQIdentityRole = IdentityServer.Modules.Accounts.IdentityRole;
using CommandQIdentityUser = IdentityServer.Modules.Accounts.IdentityUser;

namespace IdentityServer.Test.Persistence
{
    public sealed class IdentityServerPersistenceTest
    {
        [Theory]
        [InlineData(
            "PgSql",
            "Host=localhost;Database=command_q_identity;Username=test;Password=test",
            typeof(PgSqlIdentityServerDbContext),
            "Npgsql.EntityFrameworkCore.PostgreSQL")]
        [InlineData(
            "MsSql",
            "Server=localhost;Database=command_q_identity;User ID=test;Password=test;TrustServerCertificate=True",
            typeof(MsSqlIdentityServerDbContext),
            "Microsoft.EntityFrameworkCore.SqlServer")]
        [InlineData(
            "MySql",
            "Server=localhost;Database=command_q_identity;User ID=test;Password=test",
            typeof(MySqlIdentityServerDbContext),
            "MySql.EntityFrameworkCore")]
        [InlineData(
            "SqLite",
            "Data Source=command_q_identity.test.db",
            typeof(SqLiteIdentityServerDbContext),
            "Microsoft.EntityFrameworkCore.Sqlite")]
        public void Registration_Should_Resolve_Configured_Provider(
            string engine,
            string connectionString,
            Type expectedContextType,
            string expectedProviderName)
        {
            var configuration = CreateConfiguration(engine, connectionString);
            var services = new ServiceCollection();

            services.AddLogging();
            IdentityBuilder identityBuilder = services
                .AddIdentityCore<CommandQIdentityUser>()
                .AddRoles<CommandQIdentityRole>();

            services.AddIdentityServerPersistence(configuration, identityBuilder);

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<IdentityServerDbContext>();

            Assert.Equal(engine, configuration.GetIdentityDatabaseSettings().Engine.ToString());
            Assert.IsType(expectedContextType, database);
            Assert.Equal(expectedProviderName, database.Database.ProviderName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Oracle")]
        public void Configuration_Should_Reject_Unsupported_Provider(string? engine)
        {
            var configuration = CreateConfiguration(engine, "Data Source=test.db");

            var exception = Assert.Throws<InvalidOperationException>(
                configuration.GetIdentityDatabaseSettings);

            Assert.Contains("PgSql, MsSql, MySql, SqLite", exception.Message);
        }

        [Fact]
        public async Task SqLite_Initial_Migration_Should_Create_Identity_Stores()
        {
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync(TestContext.Current.CancellationToken);

            var options = new DbContextOptionsBuilder<SqLiteIdentityServerDbContext>()
                .UseSqlite(connection)
                .UseOpenIddict()
                .Options;

            await using var database = new SqLiteIdentityServerDbContext(options);
            await database.Database.MigrateAsync(TestContext.Current.CancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN " +
                "('AspNetUsers', 'OpenIddictApplications', 'IdentitySecurityEvent');";

            long tableCount = (long)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken) ?? 0L);

            Assert.Equal(3, tableCount);
        }

        [Fact]
        public void Provider_Models_Should_Match_Their_Migration_Snapshots()
        {
            using var pgSql = new PgSqlIdentityServerDbContextFactory().CreateDbContext([]);
            using var msSql = new MsSqlIdentityServerDbContextFactory().CreateDbContext([]);
            using var mySql = new MySqlIdentityServerDbContextFactory().CreateDbContext([]);
            using var sqLite = new SqLiteIdentityServerDbContextFactory().CreateDbContext([]);

            Assert.False(pgSql.Database.HasPendingModelChanges());
            Assert.False(msSql.Database.HasPendingModelChanges());
            Assert.False(mySql.Database.HasPendingModelChanges());
            Assert.False(sqLite.Database.HasPendingModelChanges());
        }

        private static IConfiguration CreateConfiguration(string? engine, string connectionString)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Engine"] = engine,
                    ["ConnectionStrings:IdentityDb"] = connectionString
                })
                .Build();
        }
    }
}
