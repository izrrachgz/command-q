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

            Assert.Equal(3, scripts.Count);
            Assert.Collection(
                scripts,
                baseline =>
                {
                    Assert.Equal(1, baseline.MigrationId);
                    Assert.Equal("migration-1.sql", baseline.Name);
                    Assert.False(string.IsNullOrWhiteSpace(baseline.Content));
                    Assert.Equal(64, baseline.Checksum.Length);
                },
                profiles =>
                {
                    Assert.Equal(2, profiles.MigrationId);
                    Assert.Equal("migration-2.sql", profiles.Name);
                    Assert.Contains("ProfileName", profiles.Content, StringComparison.Ordinal);
                    Assert.Equal(64, profiles.Checksum.Length);
                },
                traces =>
                {
                    Assert.Equal(3, traces.MigrationId);
                    Assert.Equal("migration-3.sql", traces.Name);
                    Assert.Contains("TraceParent", traces.Content, StringComparison.Ordinal);
                    Assert.Contains("TraceState", traces.Content, StringComparison.Ordinal);
                    Assert.Equal(64, traces.Checksum.Length);
                });
        }
    }
}
