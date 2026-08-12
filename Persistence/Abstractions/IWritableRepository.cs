using SqlKata.Execution;

namespace Persistence.Abstractions
{
    /// <summary>
    /// Provides a database entry point intended for write operations and migrations.
    /// </summary>
    public interface IWritableRepository : IReadOnlyRepository
    {
        /// <summary>
        /// Database query entry point with write-capable SqlKata operations.
        /// </summary>
        QueryFactory Db { get; }
    }
}
