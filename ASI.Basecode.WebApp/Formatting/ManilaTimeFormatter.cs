using System;
using System.Globalization;

namespace ASI.Basecode.WebApp.Formatting
{
    /// <summary>Formats UTC database timestamps for display in Asia/Manila.</summary>
    public static class ManilaTimeFormatter
    {
        private static readonly TimeZoneInfo Manila = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

        public static string Format(DateTime value) =>
            Format(new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));

        public static string Format(DateTime? value) => value.HasValue ? Format(value.Value) : string.Empty;

        public static string Format(DateTimeOffset value) =>
            TimeZoneInfo.ConvertTime(value, Manila).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " Asia/Manila";

        public static string Format(DateTimeOffset? value) => value.HasValue ? Format(value.Value) : string.Empty;
    }
}
