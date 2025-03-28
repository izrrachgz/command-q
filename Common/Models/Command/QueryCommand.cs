#nullable enable
namespace Common.Models.Command
{
    public class QueryCommand
    {
        public required string Query { get; set; }
        public required Type ReturnType { get; set; }
        public Dictionary<string, object?>? Parameters { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
