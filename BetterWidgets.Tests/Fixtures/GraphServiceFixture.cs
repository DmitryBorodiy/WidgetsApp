using BetterWidgets.Extensions;
using BetterWidgets.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Tests.Fixtures
{
    public class GraphServiceFixture
    {
        public IServiceProvider Services { get; private set; }

        public GraphServiceFixture() => Services = CreateServiceProvider();

        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddConfiguration();
            services.AddSettings();
            services.AddTestDataService();
            services.AddTestPermissionManager();
            services.AddMsalService();
            services.AddMSGraphService();
            services.AddTestMSCalendarService();

            return services.BuildServiceProvider();
        }
    }
}
