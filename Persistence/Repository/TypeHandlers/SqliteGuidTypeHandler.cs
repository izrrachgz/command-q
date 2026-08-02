using Dapper;
using System.Data;

namespace Persistence.Repository.TypeHandlers
{
    internal sealed class SqliteGuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        private const int GuidByteLength = 16;

        public static SqliteGuidTypeHandler Instance { get; } = new();

        private SqliteGuidTypeHandler()
        {
        }

        public override Guid Parse(object value)
        {
            return value switch
            {
                Guid guid => guid,
                byte[] bytes when bytes.Length == GuidByteLength => new Guid(bytes),
                string text when Guid.TryParse(text, out var guid) => guid,
                _ => throw new DataException($"Cannot convert {value.GetType().Name} to {nameof(Guid)}.")
            };
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.DbType = DbType.Guid;
            parameter.Value = value;
        }
    }
}
