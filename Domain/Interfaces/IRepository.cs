using SqlKata.Execution;

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
    public interface IRepository
    {
        /// <summary>
        /// Should be the database entry point
        /// </summary>
        public QueryFactory Db { get; }
    }
}
