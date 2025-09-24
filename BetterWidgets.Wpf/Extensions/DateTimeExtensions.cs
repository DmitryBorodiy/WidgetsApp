using System.Globalization;

namespace BetterWidgets.Extensions
{
    public static class DateTimeExtensions
    {
        public static string GetTimeDifferenceString(this DateTime? futureDateUtc)
        {
            DateTime now = DateTime.UtcNow;

            if(futureDateUtc <= now) return string.Empty;

            int years = 0, months = 0;
            DateTime tempNow = now;

            while(tempNow.AddYears(1) <= futureDateUtc)
            {
                tempNow = tempNow.AddYears(1);
                years++;
            }

            while(tempNow.AddMonths(1) <= futureDateUtc)
            {
                tempNow = tempNow.AddMonths(1);
                months++;
            }

            TimeSpan? remaining = futureDateUtc - tempNow;
            int? days = remaining?.Days;
            int? hours = remaining?.Hours;
            int? minutes = remaining?.Minutes;

            var parts = new List<string>();

            if(years > 0) parts.Add($"{years}y");
            if(months > 0) parts.Add($"{months}m");
            if(days > 0) parts.Add($"{days}d");
            if(hours > 0) parts.Add($"{hours}h");
            if(minutes > 0) parts.Add($"{minutes}m");

            return string.Join(" ", parts);
        }

        public static string ToDateTimeLabel(this DateTime dateTime, string format, string timeFormat, CultureInfo culture, bool showTime = false)
        {
            bool isToday = dateTime.Date == DateTime.Now.Date;
            bool isTomorrow = dateTime.Date == DateTime.Now.AddDays(1).Date;
            bool isYesterday = dateTime.Date == DateTime.Now.AddDays(-1).Date;

            var time = showTime ? $", {dateTime.ToString(timeFormat)}" : string.Empty;

            if(isToday) return $"{Resources.Resources.TodayLabel}{time}";
            if(isTomorrow) return $"{Resources.Resources.TomorrowLabel}{time}";
            if(isYesterday) return $"{Resources.Resources.YesterdayLabel}{time}";

            return dateTime.ToString(format, culture);
        }
    }
}
