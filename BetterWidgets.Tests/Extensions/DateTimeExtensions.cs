using BetterWidgets.Tests.Consts;

namespace BetterWidgets.Tests.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToUtcDateTimeString(this DateTime dateTime) 
            => dateTime.ToString(DateTimeFormats.Utc);
    }
}
