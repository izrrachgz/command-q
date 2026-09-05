using Common.Enumerations;

namespace Persistence.Extensions
{
    /// <summary>
    /// Describes native database-security capabilities used by Persistence consumers.
    /// </summary>
    public static class DatabaseEngineSecurityExtensions
    {
        /// <summary>
        /// Returns whether the engine supports the native account RLS implementation.
        /// </summary>
        public static bool SupportsNativeRowLevelSecurity(this DatabaseEngine engine)
        {
            return engine is DatabaseEngine.PgSql or DatabaseEngine.MsSql;
        }
    }
}
