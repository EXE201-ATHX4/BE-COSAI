namespace Core.Utils
{
    public class DateTimeParsing
    {
        public static DateTimeOffset ConvertToTimeZone(DateTimeOffset dateTimeOffset)
        {
            // Get the target time zone information
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Convert the given DateTimeOffset to the target time zone
            DateTimeOffset targetTime = TimeZoneInfo.ConvertTime(dateTimeOffset, targetTimeZone);
            return targetTime;
        }
        public static DateTimeOffset ConvertToUtcPlus7(DateTimeOffset dateTimeOffset)
        {
            // UTC+7 is 7 hours ahead of UTC
            TimeSpan utcPlus7Offset = new TimeSpan(7, 0, 0);
            return dateTimeOffset.ToOffset(utcPlus7Offset);
        }
        public static DateTimeOffset ConvertToUtcPlus7NotChanges(DateTimeOffset dateTimeOffset)
        {
            // UTC+7 is 7 hours ahead of UTC
            TimeSpan utcPlus7Offset = new TimeSpan(7, 0, 0);
            return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, dateTimeOffset.Second, utcPlus7Offset);
        }
    }
}
