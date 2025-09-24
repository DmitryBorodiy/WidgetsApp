using Windows.Security.Credentials.UI;

namespace BetterWidgets.Services
{
    public interface IWindowsHelloService
    {
        bool IsLocked { get; }
        TimeSpan LockTime { get; set; }

        void SetLockTime(TimeSpan lockTime);
        Task<bool> CheckAvailabilityAsync();
        Task<UserConsentVerificationResult> RequestConcentAsync(string message = default, CancellationToken token = default);
    }
}
