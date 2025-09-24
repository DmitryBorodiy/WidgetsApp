namespace BetterWidgets.Extensions
{
    public static class StringExtensions
    {
        public static Uri ToUri(this string value)
        {
            if(!Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute)) return null;

            return new Uri(value);
        }
    }
}
