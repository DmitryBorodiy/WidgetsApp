namespace BetterWidgets.Helpers
{
    public class DateTimeHelper
    {
        public static IEnumerable<DateTime> GetMonthsFromYear(int year, int day) => 
            Enumerable.Range(1, 12)
            .Select(month => new DateTime(year, month, day))
            .ToList();

        public static IEnumerable<DateTime> GetAllYears(int month, int day, int minYear = 1999, int maxYear = 2050)
        {
            for(int year = minYear; year <= maxYear; year++)
            {
                DateTime date;

                try
                {
                    date = new DateTime(year, month, day);
                }
                catch(ArgumentOutOfRangeException)
                {
                    continue;
                }

                yield return date;
            }
        }
    }
}
