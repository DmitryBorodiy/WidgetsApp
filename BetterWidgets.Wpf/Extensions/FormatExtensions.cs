namespace BetterWidgets.Extensions
{
    public static class FormatExtensions
    {
        public static string ToReadable(this long value, string unit = "B", int precision = 2)
        {
            double bytes = value;

            switch(unit)
            {
                case "KB":
                    bytes *= 1024;
                    break;
                case "MB":
                    bytes *= 1024 * 1024;
                    break;
                case "GB":
                    bytes *= 1024 * 1024 * 1024;
                    break;
            }

            int order = 0;
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };

            while (bytes >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }

            return $"{(int)Math.Round(bytes, precision)} {suffixes[order]}";
        }

        public static string ToReadable(this ulong value, string unit = "B", int precision = 2)
        {
            double bytes = value;

            switch(unit)
            {
                case "KB":
                    bytes *= 1024;
                    break;
                case "MB":
                    bytes *= 1024 * 1024;
                    break;
                case "GB":
                    bytes *= 1024 * 1024 * 1024;
                    break;
            }

            int order = 0;
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };

            while (bytes >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }

            return $"{(int)Math.Round(bytes, precision)} {suffixes[order]}";
        }

        public static string ToReadable(this float value, string unit = "B", int precision = 2)
        {
            double bytes = value;

            switch(unit)
            {
                case "KB":
                    bytes *= 1024;
                    break;
                case "MB":
                    bytes *= 1024 * 1024;
                    break;
                case "GB":
                    bytes *= 1024 * 1024 * 1024;
                    break;
            }

            int order = 0;
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };

            while (bytes >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }

            return $"{(int)Math.Round(bytes, precision)} {suffixes[order]}";
        }
    }
}
