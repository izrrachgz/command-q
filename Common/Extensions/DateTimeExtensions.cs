namespace Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static int? GetAge(this DateTime? date)
        {
            if (date is null || date == DateTime.MinValue)
                return null;

            return DateTime.Now.AddYears(-date.Value.Year).Year;
        }

        public static bool HasValue(this DateTime? date)
        {
            return date is not null && date.HasValue && date.Value != DateTime.MinValue;
        }
    }
}
