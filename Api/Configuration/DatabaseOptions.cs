using Common.Enumerations;

namespace Api.Configuration
{
    internal sealed class DatabaseOptions
    {
        public string Db { get; init; } = string.Empty;
        public DatabaseEngine Engine { get; init; }
    }
}
