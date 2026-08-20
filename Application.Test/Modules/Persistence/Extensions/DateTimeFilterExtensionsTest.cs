using Common.Enumerations;
using Persistence.Repository.Extensions;

namespace Application.Test.Modules.Persistence.Extensions
{
    public sealed class DateTimeFilterExtensionsTest
    {
        [Fact]
        public void ToDateTimeFilterValue_Should_Format_Utc_For_SqLite()
        {
            var value = new DateTimeOffset(2026, 8, 17, 11, 0, 0, TimeSpan.FromHours(-7));

            var result = value.ToDateTimeFilterValue(DatabaseEngine.SqLite);

            Assert.Equal("2026-08-17 18:00:00", result);
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
    }
}
