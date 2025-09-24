using BetterWidgets.Consts;

namespace BetterWidgets.Exceptions
{
    public class NetworkUnavailableException : Exception
    {
        public NetworkUnavailableException() : base(Errors.NetworkUnavailable) { }
        public NetworkUnavailableException(string message) : base(message) { }
        public NetworkUnavailableException(string message, Exception innerException) : base(message, innerException) { }
    }
}
