using Common.Enumerations;

namespace Email.Outbox.Persistence.Telemetry
{
    /// <summary>
    /// Defines stable telemetry values owned by email outbox persistence.
    /// </summary>
    internal static class EmailOutboxPersistenceTelemetry
    {
        /// <summary>
        /// Converts a database engine to its stable, low-cardinality telemetry value.
        /// </summary>
        /// <param name="engine">The configured persistence engine.</param>
        /// <returns>A stable lowercase value suitable for traces and metrics.</returns>
        public static string GetEngine(DatabaseEngine engine)
        {
            return engine switch
            {
                DatabaseEngine.PgSql => "pgsql",
                DatabaseEngine.MsSql => "mssql",
                DatabaseEngine.MySql => "mysql",
                DatabaseEngine.SqLite => "sqlite",
                _ => "unknown"
            };
        }
    }
}
