using Dapper;
using NodaTime;
using NodaTime.Text;
using System.Data;
using Email.Outbox.Persistence.Constants;

namespace Email.Outbox.Persistence.Storage
{
    /// <summary>
    /// Converts NodaTime instants to provider-native UTC values and restores them without local-time conversion.
    /// </summary>
    internal sealed class UtcInstantTypeHandler : SqlMapper.TypeHandler<Instant>
    {
        private const string SqLiteParameterNamespace = "System.Data.SQLite";
        private static readonly InstantPattern PersistencePattern = InstantPattern.CreateWithInvariantCulture(
            EmailOutboxPersistenceConstants.UtcInstantPattern);

        public static UtcInstantTypeHandler Instance { get; } = new();

        private UtcInstantTypeHandler()
        {
        }

        public override Instant Parse(object value)
        {
            return value switch
            {
                Instant instant => instant,
                DateTime dateTime => FromDateTime(dateTime),
                DateTimeOffset dateTimeOffset => Instant.FromDateTimeOffset(dateTimeOffset),
                string text => ParseText(text),
                _ => throw new DataException($"Cannot convert {value.GetType().Name} to {nameof(Instant)}.")
            };
        }

        public override void SetValue(IDbDataParameter parameter, Instant value)
        {
            if (string.Equals(parameter.GetType().Namespace, SqLiteParameterNamespace, StringComparison.Ordinal))
            {
                parameter.DbType = DbType.String;
                parameter.Value = PersistencePattern.Format(value);
                return;
            }

            parameter.Value = value.ToDateTimeUtc();
        }

        private static Instant FromDateTime(DateTime value)
        {
            if (value.Kind == DateTimeKind.Local)
            {
                throw new DataException("Local DateTime values cannot be materialized as UTC instants.");
            }

            return Instant.FromDateTimeUtc(value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc));
        }

        private static Instant ParseText(string value)
        {
            var result = PersistencePattern.Parse(value);

            if (result.Success)
            {
                return result.Value;
            }

            throw new DataException("The database value is not a valid UTC instant.", result.Exception);
        }
    }
}
