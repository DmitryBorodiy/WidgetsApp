using System.Runtime.InteropServices;

namespace BetterWidgets.Helpers.Logging
{
    public class ConsoleHelper
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
    }
}
