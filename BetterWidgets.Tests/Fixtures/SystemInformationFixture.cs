using BetterWidgets.Extensions;
using BetterWidgets.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Tests.Fixtures
{
    public class SystemInformationFixture
    {
        public SystemInformationFixture() => Services = InjectDependenices();

        public IServiceProvider Services { get; private set; }

        private IServiceProvider InjectDependenices()
        {
            var builder = new ServiceCollection();

            builder.AddLogging();
            builder.AddSettings();
            builder.AddTestPermissionManager();
            builder.AddSystemInformation();
            builder.AddTestDataService();
            builder.AddGpuWatcher();
            builder.AddCpuWatcher();

            return builder.BuildServiceProvider();
        }
    }
}
