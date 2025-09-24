using BetterWidgets.Extensions;
using BetterWidgets.Services;
using BetterWidgets.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Tests.Fixtures
{
    public class TodoManagerFixture
    {
        public IServiceProvider Services { get; private set; }

        public TodoManagerFixture()
        {
            Services = CreateServiceProvider();

            SignInAsync().GetAwaiter().GetResult();
        }

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
            services.AddTodoManager();

            return services.BuildServiceProvider();
        }

        private async Task SignInAsync()
        {
            var graph = Services.GetRequiredService<IMSGraphService>();

            if(!graph.IsSignedIn)
               await graph.SignInAsync();
        }
    }
}
