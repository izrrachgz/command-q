using SqlKata;
using SqlKata.Execution;
using Common.Enumerations;
using Common.Models.Request;
using Persistence.Repository;
using Persistence.Repository.Extensions;
using Persistence.Repository.TypeHandlers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Persistence.Test.Extensions
{
    public sealed class DateTimeFilterExtensionsTest
    {
        [Fact]
        public void ToDateTimeFilterValue_Should_Format_Utc_For_SqLite()
        {
            var value = new DateTimeOffset(2026, 8, 17, 11, 0, 0, TimeSpan.FromHours(-7));

            var result = value.ToDateTimeFilterValue(DatabaseEngine.SqLite);

            Assert.Equal("2026-08-17 18:00:00.000000Z", result);
        }

        [Theory]
        [InlineData(DatabaseEngine.MsSql)]
        [InlineData(DatabaseEngine.MySql)]
        [InlineData(DatabaseEngine.PgSql)]
        public void ToDateTimeFilterValue_Should_Return_Utc_DateTime_For_Relational_Providers(DatabaseEngine engine)
        {
            var value = new DateTimeOffset(2026, 8, 17, 11, 0, 0, TimeSpan.FromHours(-7));

            var result = Assert.IsType<DateTime>(value.ToDateTimeFilterValue(engine));

            Assert.Equal(new DateTime(2026, 8, 17, 18, 0, 0, DateTimeKind.Utc), result);
        }

        [Fact]
        public async Task SqLite_Should_Store_And_Filter_Microsecond_Precision_Utc_Values()
        {
            var path = Path.Combine(Path.GetTempPath(), $"command-q-utc-{Guid.NewGuid():N}.sqlite");

            try
            {
                using var repository = new WritableRepository(
                    $"Data Source={path};Version=3;",
                    DatabaseEngine.SqLite,
                    NullLogger<WritableRepository>.Instance);
                repository.Db.Connection.Open();
                using (var command = repository.Db.Connection.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE UtcRecord (Id INTEGER PRIMARY KEY, CreatedUtc TEXT NOT NULL);";
                    command.ExecuteNonQuery();
                }

                var instant = new DateTime(2026, 8, 17, 18, 0, 0, 123, DateTimeKind.Utc).AddTicks(4567);
                var expected = instant.AddTicks(-7);
                await repository.Db.Query("UtcRecord").InsertAsync(
                    new { Id = 1, CreatedUtc = instant },
                    cancellationToken: TestContext.Current.CancellationToken);
                var storedValue = await repository.Db.Query("UtcRecord")
                    .Select("CreatedUtc")
                    .FirstAsync<string>(cancellationToken: TestContext.Current.CancellationToken);
                var criteria = new DateRangeCriteria
                {
                    StartDate = new DateTimeOffset(instant.AddTicks(-1)),
                    EndDate = new DateTimeOffset(instant.AddTicks(1))
                };
                var record = await repository.Db.Query("UtcRecord")
                    .ApplyUtcRange("CreatedUtc", criteria, DatabaseEngine.SqLite)
                    .FirstOrDefaultAsync<UtcRecord>(cancellationToken: TestContext.Current.CancellationToken);

                Assert.NotNull(record ?? throw new Xunit.Sdk.XunitException($"Stored SQLite value was '{storedValue}'."));
                Assert.Equal("2026-08-17 18:00:00.123456Z", storedValue);
                Assert.Equal(expected, record.CreatedUtc);
                Assert.Equal(DateTimeKind.Utc, record.CreatedUtc.Kind);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Theory]
        [InlineData(DateTimeKind.Local)]
        [InlineData(DateTimeKind.Unspecified)]
        public void SqLite_Should_Reject_NonUtc_DateTime_Writes(DateTimeKind kind)
        {
            var value = new DateTime(2026, 8, 17, 18, 0, 0, kind);

            Assert.Throws<InvalidOperationException>(() => SqliteUtcDateTimeTypeHandler.FormatUtc(value));
        }

        private sealed class UtcRecord
        {
            public long Id { get; set; }
            public DateTime CreatedUtc { get; set; }
        }
    }
}
