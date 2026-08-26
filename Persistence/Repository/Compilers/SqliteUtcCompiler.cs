using SqlKata;
using SqlKata.Compilers;
using Persistence.Repository.TypeHandlers;

namespace Persistence.Repository.Compilers
{
    /// <summary>
    /// Normalizes temporal bindings before SQLite receives them as text parameters.
    /// </summary>
    internal sealed class SqliteUtcCompiler : SqliteCompiler
    {
        public override string Parameter(SqlResult context, object parameter)
        {
            object normalizedParameter = parameter switch
            {
                DateTime dateTime => SqliteUtcDateTimeTypeHandler.FormatUtc(dateTime),
                DateTimeOffset dateTimeOffset => SqliteUtcDateTimeTypeHandler.FormatUtc(dateTimeOffset.UtcDateTime),
                _ => parameter
            };

            return base.Parameter(context, normalizedParameter);
        }
    }
}
