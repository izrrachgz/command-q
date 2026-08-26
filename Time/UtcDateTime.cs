namespace Time
{
    /// <summary>
    /// Enforces and restores the UTC kind on platform <see cref="DateTime"/> values.
    /// </summary>
    public static class UtcDateTime
    {
        /// <summary>
        /// Returns a UTC value or rejects values whose kind is not UTC.
        /// </summary>
        public static DateTime Require(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new InvalidOperationException($"DateTime values must use {nameof(DateTimeKind.Utc)} kind.");
            }

            return value;
        }

        /// <summary>
        /// Restores the UTC kind after a provider materializes a timezone-neutral database value.
        /// Local values are rejected because relabeling them would change the represented instant.
        /// </summary>
        public static DateTime RestoreFromDatabase(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => throw new InvalidOperationException("Local DateTime values cannot be restored as UTC database values.")
            };
        }
    }
}
