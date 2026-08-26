namespace Cqrs.Abstractions
{
    /// <summary>
    /// Represents the successful completion of a command without a result payload.
    /// </summary>
    public readonly record struct Unit
    {
        public static Unit Value { get; } = default;
    }
}
