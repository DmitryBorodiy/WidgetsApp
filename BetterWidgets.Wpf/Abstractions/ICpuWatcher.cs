namespace BetterWidgets.Abstractions
{
    public interface ICpuWatcher
    {
        bool IsStarted { get; }
        int MaxPoints { get; set; }
        List<int> Utilization { get; }
        double UpdateTick { get; set; }

        event EventHandler<List<int>> UtilizationChanged;

        Task<IEnumerable<int>> GetUtilizationDataAsync();

        void Start();
        void Stop();
    }
}
