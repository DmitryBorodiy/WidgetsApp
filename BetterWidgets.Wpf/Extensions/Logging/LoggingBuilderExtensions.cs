using BetterWidgets.Helpers.Logging;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Extensions.Logging
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AllocConsole(this ILoggingBuilder builder)
        {
            ConsoleHelper.AllocConsole();

            return builder;
        }
    }
}
