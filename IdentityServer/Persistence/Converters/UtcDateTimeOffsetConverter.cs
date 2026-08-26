using Time;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdentityServer.Persistence.Converters
{
    internal sealed class UtcDateTimeOffsetConverter()
        : ValueConverter<DateTimeOffset, DateTimeOffset>(
            value => UtcTime.TruncateToPersistencePrecision(value),
            value => UtcTime.TruncateToPersistencePrecision(value))
    {
    }
}
