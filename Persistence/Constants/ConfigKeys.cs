namespace Persistence.Constants
{
    /// <summary>
    /// Configuration keys used by the persistence project.
    /// </summary>
    internal static class ConfigKeys
    {
        /// <summary>
        /// Connection string and database engine configuration keys.
        /// </summary>
        public static class ConnectionStrings
        {
            /// <summary>
            /// Main database connection string name.
            /// </summary>
            public const string Database = @"Db";

            /// <summary>
            /// Configuration path containing the selected database engine.
            /// </summary>
            public const string Engine = @"ConnectionStrings:Engine";
        }

    }
}
