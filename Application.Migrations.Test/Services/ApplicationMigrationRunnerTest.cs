using SqlKata.Execution;
using Common.Enumerations;
using Persistence.Repository;
using Application.Migrations.Models;
using Application.Migrations.Services;
using Application.Migrations.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Migrations.Test.Services
{
    public sealed class ApplicationMigrationRunnerTest
    {
        [Fact]
        public async Task RunAsync_Should_Apply_The_SqLite_Baseline_Only_Once()
        {
            using var repository = CreateRepository();
            var runner = new ApplicationMigrationRunner(
                repository,
                new EmbeddedMigrationScriptSource(),
                TimeProvider.System,
                NullLogger<ApplicationMigrationRunner>.Instance);

            await runner.RunAsync(TestContext.Current.CancellationToken);
            await runner.RunAsync(TestContext.Current.CancellationToken);

            var records = await repository.Db.Query(nameof(MigrationRecord))
                .GetAsync<MigrationRecord>(cancellationToken: TestContext.Current.CancellationToken);
            var record = Assert.Single(records);

            Assert.Equal(1, record.MigrationId);
            Assert.Equal("Core", record.Module);
            Assert.Equal("migration-1", record.Name);
        }

        [Fact]
        public async Task RunAsync_Should_Reject_A_Gap_In_The_Embedded_Migration_Sequence()
        {
            using var repository = CreateRepository();
            MigrationScript[] scripts =
            [
                CreateScript(1),
                CreateScript(3)
            ];
            var runner = new ApplicationMigrationRunner(
                repository,
                new StubMigrationScriptSource(scripts),
                TimeProvider.System,
                NullLogger<ApplicationMigrationRunner>.Instance);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => runner.RunAsync(TestContext.Current.CancellationToken));

            Assert.Contains("migration ID 2", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RunAsync_Should_Reject_A_Database_Ledger_Ahead_Of_The_Assembly()
        {
            using var repository = CreateRepository();
            var runner = new ApplicationMigrationRunner(
                repository,
                new EmbeddedMigrationScriptSource(),
                TimeProvider.System,
                NullLogger<ApplicationMigrationRunner>.Instance);

            await runner.RunAsync(TestContext.Current.CancellationToken);
            await repository.Db.Query(nameof(MigrationRecord)).InsertAsync(
                new
                {
                    MigrationId = 2,
                    Module = "Core",
                    Name = "migration-2",
                    Checksum = "test",
                    AppliedUtc = DateTime.UtcNow,
                    DeletedUtc = (DateTime?)null
                },
                cancellationToken: TestContext.Current.CancellationToken);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => runner.RunAsync(TestContext.Current.CancellationToken));

            Assert.Contains("ledger is at 2", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static WritableRepository CreateRepository()
        {
            return new WritableRepository(
                "Data Source=:memory:;Version=3;New=True;",
                DatabaseEngine.SqLite,
                NullLogger<WritableRepository>.Instance);
        }

        private static MigrationScript CreateScript(long migrationId)
        {
            return new MigrationScript(
                migrationId,
                "Core",
                $"migration-{migrationId}.sql",
                $"migration-{migrationId}.sql",
                string.Empty,
                "test");
        }

        private sealed class StubMigrationScriptSource(IReadOnlyList<MigrationScript> scripts)
            : IMigrationScriptSource
        {
            public Task<IReadOnlyList<MigrationScript>> GetScriptsAsync(
                DatabaseEngine engine,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(scripts);
            }
        }
    }
}
