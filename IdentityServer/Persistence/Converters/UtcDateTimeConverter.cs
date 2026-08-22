using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdentityServer.Persistence.Converters
{
    internal sealed class UtcDateTimeConverter()
        : ValueConverter<DateTime, DateTime>(
            value => RequireUtc(value),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
        private static DateTime RequireUtc(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new InvalidOperationException(
                    $"IdentityServer DateTime values must use {nameof(DateTimeKind.Utc)} kind.");
            }

            return value;
        }
    }
}
