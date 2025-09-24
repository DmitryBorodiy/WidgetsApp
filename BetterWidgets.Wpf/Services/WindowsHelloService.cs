using BetterWidgets.Consts;
using BetterWidgets.Properties;
using Microsoft.Extensions.Logging;
using System.Windows.Threading;
using Windows.Security.Credentials.UI;

namespace BetterWidgets.Services
{
    public class WindowsHelloService : IWindowsHelloService
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings _settings;
        private readonly DispatcherTimer _lockTimer;
        #endregion

        public WindowsHelloService(ILogger<WindowsHelloService> logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
            _lockTimer = CreateTimer();
        }

        #region Props

        public bool IsLocked { get; private set; } = true;

        public TimeSpan LockTime
        {
            get => _lockTimer?.Interval ??
                   _settings?.LockTime ?? default;
            set
            {
                if(_settings != null) _settings.LockTime = value;
                if(_lockTimer != null) _lockTimer.Interval = value;
            }
        }

        #endregion

        #region Methods

        private DispatcherTimer CreateTimer()
        {
            var timer = new DispatcherTimer()
            {
                Interval = _settings?.LockTime ?? default
            };

            timer.Tick += Timer_Tick;

            return timer;
        }

        public void SetLockTime(TimeSpan lockTime)
        {
            if(_settings == null) throw new InvalidOperationException(Errors.SettingsServiceNotLoaded);
            if(_lockTimer == null) throw new InvalidOperationException(Errors.LockTimerIsNotLoaded);

            _settings.LockTime = lockTime;
            _lockTimer.Interval = lockTime;
        }

        public async Task<bool> CheckAvailabilityAsync()
        {
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();

            return (availability == UserConsentVerifierAvailability.Available ||
                    availability == UserConsentVerifierAvailability.DeviceBusy) &&
                   (_settings?.GetValue<bool>("IsWindowsHelloEnabled", false) ?? false);
        }

        public async Task<UserConsentVerificationResult> RequestConcentAsync(string message = default, CancellationToken token = default)
        {
            try
            {
                if(!IsLocked) return UserConsentVerificationResult.Verified;

                var result = await UserConsentVerifier.RequestVerificationAsync(message);

                if(token.IsCancellationRequested) 
                   return UserConsentVerificationResult.Canceled;

                if(result == UserConsentVerificationResult.Verified) Unlock();

                return result;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return UserConsentVerificationResult.Canceled;
            }
        }

        private void Unlock()
        {
            IsLocked = false;

            _lockTimer?.Start();
        }

        #endregion

        #region EventHandler

        private void Timer_Tick(object sender, EventArgs e)
        {
            IsLocked = true;

            _lockTimer?.Stop();
        }

        #endregion
    }
}
