using Windows.Networking.Connectivity;

namespace BetterWidgets.Helpers
{
    public class NetworkHelper
    {
        public static bool IsConnected => CheckConnection();

        private static bool CheckConnection()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();

            return profile?.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
        }
    }
}
