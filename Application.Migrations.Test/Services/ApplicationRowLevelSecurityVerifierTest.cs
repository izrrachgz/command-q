using Common.Enumerations;
using Persistence.Repository;
using Persistence.Extensions;
using Application.Migrations.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Migrations.Test.Services
{
    public sealed class ApplicationRowLevelSecurityVerifierTest
    {
        [Theory]
        [InlineData(DatabaseEngine.MsSql, true)]
        [InlineData(DatabaseEngine.PgSql, true)]
        [InlineData(DatabaseEngine.MySql, false)]
        [InlineData(DatabaseEngine.SqLite, false)]
        public void SupportsNativeRowLevelSecurity_Should_Match_Provider_Capabilities(
            DatabaseEngine engine,
            bool expected)
        {
            Assert.Equal(expected, engine.SupportsNativeRowLevelSecurity());
        }

        [Theory]
        [InlineData(DatabaseEngine.MySql, "Server=unused;")]
        [InlineData(DatabaseEngine.SqLite, "Data Source=:memory:;Version=3;New=True;")]
        public async Task VerifyAsync_Should_Skip_Engines_Without_Native_Row_Level_Security(
            DatabaseEngine engine,
            string connectionString)
        {
            using var repository = new WritableRepository(
                connectionString,
                engine,
                NullLogger<WritableRepository>.Instance);
            var verifier = new ApplicationRowLevelSecurityVerifier(
                repository,
                NullLogger<ApplicationRowLevelSecurityVerifier>.Instance);

            await verifier.VerifyAsync(TestContext.Current.CancellationToken);
        }
    }
}
