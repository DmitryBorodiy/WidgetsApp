using System.Diagnostics;
using System.Windows.Threading;
using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Properties;
using BetterWidgets.Widgets;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Services
{
    public sealed class CpuWatcher : ICpuWatcher
    {
        #region Services
        private readonly ILogger _logger;
        private readonly DataService<CpuWidget> _data;
        private readonly PerformanceCounter _counter;
        private readonly Settings<CpuWatcher> _settings;
        private readonly DispatcherTimer _timer;
        #endregion

        public CpuWatcher(ILogger<CpuWatcher> logger, Settings<CpuWatcher> settings, DataService<CpuWidget> data, PerformanceCounter counter)
        {
            _data = data;
            _logger = logger;
            _counter = counter;
            _settings = settings;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(UpdateTick);
            _timer.Tick += OnTick;

            Utilization = new List<int>();
        }

        #region Props

        public bool IsStarted { get; private set; }
        public int MaxPoints { get; set; } = 30;

        public double UpdateTick
        {
            get => _settings.GetSetting<double>(nameof(UpdateTick), 1000);
            set
            {
                _settings.SetSetting<double>(nameof(UpdateTick), value);

                if(_timer != null && value > 500)
                   _timer.Interval = TimeSpan.FromMilliseconds(value);
            }
        }

        public List<int> Utilization { get; private set; }

        #endregion

        #region Events
        public event EventHandler<List<int>> UtilizationChanged;
        #endregion

        #region Methods

        public void Start()
        {
            if(IsStarted) return;
            else IsStarted = true;

                _counter.NextValue();
            Task.Delay(1000).ContinueWith(_ => _timer.Start());
        }

        public void Stop()
        {
            if(!IsStarted) return;
            else IsStarted = false;

            _timer?.Stop();
        }

        public async Task<IEnumerable<int>> GetUtilizationDataAsync()
        {
            try
            {
                if(Utilization != null && Utilization.Count > 0) return Utilization;

                var data = await _data.GetFromFileAsync<List<int>>(FileNames.cpuUtilization);

                if(data.ex != null) throw data.ex;

                Utilization = data.data ?? new List<int>();

                return Utilization;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return Enumerable.Empty<int>();
            }
        }

        private async Task SaveUtilizationDataAsync()
        {
            try
            {
                if(Utilization == null)
                   Utilization = new List<int>();

                await _data.SetToFileAsync(FileNames.cpuUtilization, Utilization);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        private async void OnTick(object sender, EventArgs e)
        {
            try
            {
                if(Utilization.Count >= MaxPoints)
                   Utilization.RemoveAt(0);

                int nextValue = (int)Math.Round(_counter.NextValue());
                Utilization.Add(nextValue);

                UtilizationChanged?.Invoke(this, Utilization);

                await SaveUtilizationDataAsync();
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        #endregion
    }
}
