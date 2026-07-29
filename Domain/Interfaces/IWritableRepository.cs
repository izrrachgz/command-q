using SqlKata.Execution;

namespace Domain.Interfaces
{
    /// <summary>
    /// Provides a database entry point intended for write operations.
    /// </summary>
    public interface IWritableRepository : IReadOnlyRepository
    {
        /// <summary>
        /// Database query entry point with write-capable SqlKata operations.
        /// </summary>
        QueryFactory Db { get; }

        /// <summary>
        /// Associated migrations path.
        /// </summary>
        string MigrationsPath { get; }
    }
}
