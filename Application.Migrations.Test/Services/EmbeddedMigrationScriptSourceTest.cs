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

        [Fact]
        public async Task GetScriptsAsync_Should_Include_PostgreSql_Row_Level_Security()
        {
            var source = new EmbeddedMigrationScriptSource();

            var scripts = await source.GetScriptsAsync(DatabaseEngine.PgSql, TestContext.Current.CancellationToken);
            string content = Assert.Single(scripts).Content;

            Assert.Equal(10, CountOccurrences(content, "CREATE POLICY \"AccountIsolationPolicy\""));
            Assert.Equal(10, CountOccurrences(content, "ENABLE ROW LEVEL SECURITY"));
            Assert.Equal(10, CountOccurrences(content, "FORCE ROW LEVEL SECURITY"));
            Assert.Contains("security.can_access_account", content, StringComparison.Ordinal);
            Assert.Contains("security.can_access_register", content, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetScriptsAsync_Should_Include_SqlServer_Row_Level_Security()
        {
            var source = new EmbeddedMigrationScriptSource();

            var scripts = await source.GetScriptsAsync(DatabaseEngine.MsSql, TestContext.Current.CancellationToken);
            string content = Assert.Single(scripts).Content;

            Assert.Equal(10, CountOccurrences(content, "ADD FILTER PREDICATE"));
            Assert.Equal(20, CountOccurrences(content, "ADD BLOCK PREDICATE"));
            Assert.Contains("CREATE SECURITY POLICY Security.AccountIsolationPolicy", content, StringComparison.Ordinal);
            Assert.Contains("WITH (STATE = ON, SCHEMABINDING = ON)", content, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(DatabaseEngine.MySql)]
        [InlineData(DatabaseEngine.SqLite)]
        public async Task GetScriptsAsync_Should_Not_Claim_Native_Row_Level_Security_For_Unsupported_Engines(DatabaseEngine engine)
        {
            var source = new EmbeddedMigrationScriptSource();

            var scripts = await source.GetScriptsAsync(engine, TestContext.Current.CancellationToken);
            string content = Assert.Single(scripts).Content;

            Assert.DoesNotContain("ROW LEVEL SECURITY", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("SECURITY POLICY", content, StringComparison.OrdinalIgnoreCase);
        }

        private static int CountOccurrences(string value, string searchValue)
        {
            return value.Split(searchValue, StringSplitOptions.None).Length - 1;
        }
    }
}
