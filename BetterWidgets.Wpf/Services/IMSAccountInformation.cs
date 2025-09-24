using BetterWidgets.Model;

namespace BetterWidgets.Services
{
    public interface IMSAccountInformation
    {
        bool IsSignedIn { get; }

        Task<Account> GetAccountInformationAsync();
        Task FlushDataAsync();
    }
}
