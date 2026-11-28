using Time;
using SqlKata.Execution;
using Common.Enumerations;
using Common.Models.Request;
using Persistence.Repository;
using Persistence.Repository.Command;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Persistence.Test;

public sealed class SqliteCommandRunnerUtcTest
{
    private static readonly DateTime Sample = new DateTime(2026, 8, 17, 18, 0, 0, 123, DateTimeKind.Utc).AddTicks(4567);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Bulk_Insert_Should_Match_The_Same_Utc_Range_As_SqlKata(bool callerOwnsTransaction)
    {
        var path = Path.Combine(Path.GetTempPath(), $"commandq-utc-{Guid.NewGuid():N}.sqlite");
        var cancellationToken = TestContext.Current.CancellationToken;

        try
        {
            string connection = $"Data Source={path};Version=3;";
            using var repository = CreateRepository(connection);
            var runner = new SqLiteCommandRunner(connection, NullLogger<SqLiteCommandRunner>.Instance);
            string[] columns = ["Id", "CreatedUtc"];
            Dictionary<string, object?>[] rows =
            [
                new() { ["Id"] = 1, ["CreatedUtc"] = Sample },
                new() { ["Id"] = 2, ["CreatedUtc"] = new DateTimeOffset(Sample).ToOffset(TimeSpan.FromHours(-7)) }
            ];

            if (callerOwnsTransaction)
            {
                using var transaction = repository.Db.Connection.BeginTransaction();
                var result = await runner.BulkInsert("UtcRecord", columns, rows, transaction, cancellationToken);
                Assert.True(result.Success);
                await repository.Db.Query("UtcRecord").InsertAsync(
                    new { Id = 3, CreatedUtc = Sample }, transaction, cancellationToken: cancellationToken);
                transaction.Commit();
            }
            else
            {
                var result = await runner.BulkInsert("UtcRecord", columns, rows, cancellationToken);
                Assert.True(result.Success);
            }

            var criteria = new DateRangeCriteria { StartDate = Sample, EndDate = Sample };
            var values = await repository.Db.Query("UtcRecord")
                .Select("CreatedUtc")
                .ApplyUtcRange("CreatedUtc", criteria, DatabaseEngine.SqLite)
                .GetAsync<string>(cancellationToken: cancellationToken);
            Assert.Equal(callerOwnsTransaction ? 3 : 2, values.Count());
            Assert.All(values, value => Assert.Equal(UtcTime.FormatForPersistence(Sample), value));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("2026-08-17 18:00:00.1234567Z", 1234560)]
    [InlineData("2026-08-17 18:00:00.12Z", 1200000)]
    [InlineData("2026-08-17 18:00:00Z", 0)]
    public async Task Repository_Should_Read_Legacy_Utc_Text_At_Canonical_Precision(string storedValue, long fractionTicks)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var repository = CreateRepository("Data Source=:memory:;Version=3;");

        await repository.Db.Query("UtcRecord").InsertAsync(new { Id = 1, CreatedUtc = storedValue }, cancellationToken: cancellationToken);
        var record = await repository.Db.Query("UtcRecord").FirstAsync<UtcRecord>(cancellationToken: cancellationToken);
        Assert.Equal(new DateTime(2026, 8, 17, 18, 0, 0, DateTimeKind.Utc).AddTicks(fractionTicks), record.CreatedUtc);
        Assert.Equal(DateTimeKind.Utc, record.CreatedUtc.Kind);
    }

    private static WritableRepository CreateRepository(string connectionString)
    {
        var repository = new WritableRepository(connectionString, DatabaseEngine.SqLite, NullLogger<WritableRepository>.Instance);
        repository.Db.Connection.Open();
        using var command = repository.Db.Connection.CreateCommand();
        command.CommandText = "CREATE TABLE UtcRecord (Id INTEGER PRIMARY KEY, CreatedUtc TEXT NOT NULL)";
        command.ExecuteNonQuery();
        return repository;
    }

    private sealed class UtcRecord
    {
        public DateTime CreatedUtc { get; set; }
    }
}
