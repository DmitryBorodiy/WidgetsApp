using Microsoft.Identity.Client;

namespace BetterWidgets.Services
{
    public interface IMsalService
    {
        Task<IAccount> TryGetAccountAsync(string accountId = default);
        Task<IEnumerable<IAccount>> TryGetAllAccountsAsync();
        Task<AuthenticationResult> AuthenticateAsync(string accoutId = default, CancellationToken token = default);
        Task ForgetTokenAndSignOutAsync(string accountId = default);
    }
}
