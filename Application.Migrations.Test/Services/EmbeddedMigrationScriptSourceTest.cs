using Common.Enumerations;
using Application.Migrations.Services;

namespace Application.Migrations.Test.Services
{
    public sealed class EmbeddedMigrationScriptSourceTest
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
        public async Task GetScriptsAsync_Should_Load_The_VersionOne_Baseline(DatabaseEngine engine)
        {
            var source = new EmbeddedMigrationScriptSource();

            var scripts = await source.GetScriptsAsync(engine, TestContext.Current.CancellationToken);

            var script = Assert.Single(scripts);
            Assert.Equal(1, script.MigrationId);
            Assert.Equal("Core", script.Module);
            Assert.Equal("migration-1.sql", script.Name);
            Assert.False(string.IsNullOrWhiteSpace(script.Content));
            Assert.Equal(64, script.Checksum.Length);
        }
    }
}
