using SqlKata.Execution;
using Common.Enumerations;

namespace Domain.Interfaces
{
    /// <summary>
    /// Should provide a database entry point with read access
    /// </summary>
    public interface IReadOnlyRepository
    {
        /// <summary>
        /// Should be the database entry point
        /// </summary>
        public QueryFactory Db { get; }
    }

    /// <summary>
    /// Should provide a database entry point with write/read access
    /// </summary>
    public interface IWritableRepository
    {
        /// <summary>
        /// Should be the database entry point
        /// </summary>
        public QueryFactory Db { get; }

        /// <summary>
        /// Should be the asociated database engine
        /// </summary>
        public DatabaseEngine Engine { get; }

        /// <summary>
        /// Should be the asociated migrations path
        /// </summary>
        public string MigrationsPath { get; }
    }
}
