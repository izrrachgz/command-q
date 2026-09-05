namespace Persistence.Abstractions
{
    /// <summary>
    /// Represents one provider-neutral database-session value.
    /// </summary>
    /// <param name="Key">Provider session key.</param>
    /// <param name="Value">Value to store, or <see langword="null"/> to clear it.</param>
    public sealed record DatabaseSessionValue(string Key, object? Value);
}
