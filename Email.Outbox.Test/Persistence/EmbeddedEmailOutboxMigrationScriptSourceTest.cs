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
        public async Task Source_Should_Load_A_Baseline_For_Each_Engine(DatabaseEngine engine)
        {
            var source = new EmbeddedEmailOutboxMigrationScriptSource();

            var scripts = await source.GetScriptsAsync(
                engine,
                TestContext.Current.CancellationToken);

            var script = Assert.Single(scripts);
            Assert.Equal(1, script.MigrationId);
            Assert.Equal("migration-1.sql", script.Name);
            Assert.False(string.IsNullOrWhiteSpace(script.Content));
            Assert.Equal(64, script.Checksum.Length);
        }
    }
}
