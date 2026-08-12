namespace Api.Configuration
{
    public sealed record ApiClientOptions
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
    }
}
