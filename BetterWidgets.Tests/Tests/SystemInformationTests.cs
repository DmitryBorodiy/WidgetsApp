using Xunit.Abstractions;
using BetterWidgets.Abstractions;
using BetterWidgets.Extensions;
using BetterWidgets.Services;
using BetterWidgets.Tests.Fixtures;
using BetterWidgets.Tests.Widgets;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Tests
{
    public class SystemInformationTests : IClassFixture<SystemInformationFixture>
    {
        #region Services
        private readonly ICpuWatcher _cpuWatcher;
        private readonly ITestOutputHelper _output;
        private readonly IPerformanceWatcher<GpuWidget> _gpuWatcher;
        private readonly ISystemInformation<CpuWidget> _sysInfo;
        #endregion

        public SystemInformationTests(SystemInformationFixture fixture, ITestOutputHelper output)
        {
            _output = output;
            _cpuWatcher = fixture.Services?.GetRequiredService<ICpuWatcher>();
            _sysInfo = fixture.Services?.GetRequiredService<ISystemInformation<CpuWidget>>();
            _gpuWatcher = fixture.Services?.GetRequiredKeyedService<IPerformanceWatcher<GpuWidget>>(nameof(GpuPerformanceWatcher<IWidget>));
        }

        [Fact]
        public async Task Should_Get_CpuInformation()
        {
            var info = await _sysInfo.GetCpuInformationAsync();

            if(info.ex != null) throw info.ex;

            Assert.NotNull(info.cpu);

            _output.WriteLine(info.cpu.ToString());
        }

        [Fact]
        public async Task Should_Get_GpuInformation()
        {
            var gpusResult = await _sysInfo.GetGpuInformationAsync();

            if(gpusResult.ex != null) throw gpusResult.ex;

            Assert.NotNull(gpusResult.gpus);

            foreach(var gpu in gpusResult.gpus)
                _output.WriteLine(gpu.ToString());
        }

        [Fact]
        public async Task Should_Get_Gpu_Utilization_Percent()
        {
            await _gpuWatcher.GetUtilizationReportAsync();

            var report = await _gpuWatcher.GetUtilizationReportAsync();

            if(report.ex != null) throw report.ex;

            Assert.NotNull(report.report);
            Assert.NotEmpty(report.report);

            foreach(var gpu in report.report)
            {
                _output.WriteLine(gpu.Name);
                _output.WriteLine("{0}%", (int)Math.Round(gpu.Load));
                _output.WriteLine("Total memory: {0}", gpu.MemoryTotal.ToReadable("MB"));
                _output.WriteLine("Memory usage: {0}", gpu.MemoryUsage.ToReadable("MB"));
            }
        }
    }
}
