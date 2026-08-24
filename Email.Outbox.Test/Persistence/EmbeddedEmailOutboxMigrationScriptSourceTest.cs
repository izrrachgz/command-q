using Common.Enumerations;
using Email.Outbox.Persistence.Migrations;

namespace Email.Outbox.Test.Persistence
{
    public sealed class EmbeddedEmailOutboxMigrationScriptSourceTest
    {
        public static TheoryData<DatabaseEngine> SupportedEngines => new()
        {
            DatabaseEngine.MsSql,
            DatabaseEngine.PgSql,
            DatabaseEngine.MySql,
            DatabaseEngine.SqLite
        };

        [Theory]
        [MemberData(nameof(SupportedEngines))]
        public async Task Source_Should_Load_All_Migrations_For_Each_Engine(DatabaseEngine engine)
        {
            var source = new EmbeddedEmailOutboxMigrationScriptSource();

            var scripts = await source.GetScriptsAsync(
                engine,
                TestContext.Current.CancellationToken);

            var baseline = Assert.Single(scripts);
            Assert.Equal(1, baseline.MigrationId);
            Assert.Equal("migration-1.sql", baseline.Name);
            Assert.Contains("ProfileName", baseline.Content, StringComparison.Ordinal);
            Assert.Contains("TraceParent", baseline.Content, StringComparison.Ordinal);
            Assert.Contains("TraceState", baseline.Content, StringComparison.Ordinal);
            Assert.Equal(64, baseline.Checksum.Length);
        }
    }
}
