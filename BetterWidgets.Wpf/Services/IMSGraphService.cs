using Microsoft.Graph;

namespace BetterWidgets.Services
{
    public interface IMSGraphService
    {
        bool IsSignedIn { get; }
        GraphServiceClient Client { get; }

        event EventHandler SignedIn;
        event EventHandler SignedOut;

        Task<GraphServiceClient> SignInAsync(bool raiseEvent = true);
        Task SignOutAsync(bool raiseEvent = true);
    }
}
