using BetterWidgets.Model;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Services.Hardware
{
    public class GpuUpdateVisitor : IVisitor
    {
        private readonly ILogger _logger;

        public GpuUpdateVisitor(ILogger<GpuUpdateVisitor> logger)
        {
            _logger = logger;
        }

        public List<UtilizationReport> Reports { get; } = new();

        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            Reports.Clear();

            if(hardware.HardwareType == HardwareType.GpuNvidia ||
               hardware.HardwareType == HardwareType.GpuAmd ||
               hardware.HardwareType == HardwareType.GpuIntel)
            {
                hardware.Update();

                var loadSensors = hardware.Sensors
                    .Where(s => s.SensorType == SensorType.Load)
                    .ToList();
                var memorySensor = hardware.Sensors.Where(
                    s => (s.SensorType == SensorType.Load || s.SensorType == SensorType.SmallData) && s.Name.Contains("Memory Used", StringComparison.OrdinalIgnoreCase));
                var memoryTotalSensor = hardware.Sensors
                    .FirstOrDefault(s => s.SensorType == SensorType.SmallData && s.Name.Contains("Memory"));

                float load = Math.Clamp(loadSensors.Any()
                    ? loadSensors.Sum(s => s.Value ?? 0) : 0, 0f, 100f);
                float memoryUsed = memorySensor.Any() ?
                    memorySensor.Sum(s => s.Value ?? 0) : 0;

                Reports.Add(new UtilizationReport()
                {
                    Name = hardware.Name,
                    Load = load,
                    MemoryUsage = memoryUsed,
                    MemoryTotal = memoryTotalSensor?.Value ?? 0
                });
            }

            foreach(var sub in hardware.SubHardware)
                sub.Accept(this);
        }

        public void VisitParameter(IParameter parameter) { }

        public void VisitSensor(ISensor sensor)
        {
            _logger?.LogInformation($"Name: {sensor.Name} \n Type: {sensor.SensorType} \n Value: {sensor.Value}");
        }
    }
}
