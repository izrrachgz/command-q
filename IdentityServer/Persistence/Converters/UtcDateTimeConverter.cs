using Time;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdentityServer.Persistence.Converters
{
    internal sealed class UtcDateTimeConverter()
        : ValueConverter<DateTime, DateTime>(
            value => RequireUtc(value),
            value => UtcDateTime.RestoreFromDatabase(value))
    {
        private static DateTime RequireUtc(DateTime value)
        {
            return UtcTime.TruncateToPersistencePrecision(UtcDateTime.Require(value));
        }
    }
}
