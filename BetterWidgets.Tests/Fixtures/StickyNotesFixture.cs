using BetterWidgets.Extensions;
using BetterWidgets.Services;
using BetterWidgets.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Tests.Fixtures
{
    public sealed class StickyNotesFixture
    {
        public StickyNotesFixture()
        {
            Services = BuildServiceProvider();
            SignInAsync().GetAwaiter().GetResult();
        }

        public IServiceProvider Services { get; private set; }

        private IServiceProvider BuildServiceProvider()
        {
            var builder = new ServiceCollection();

            builder.AddLogging();
            builder.AddSettings();
            builder.AddConfiguration();
            builder.AddTestDataService();
            builder.AddMsalService();
            builder.AddTestPermissionManager();
            builder.AddMSGraphService();
            builder.AddStickyNotes();

            return builder.BuildServiceProvider();
        }

        private async Task SignInAsync()
        {
            var graph = Services?.GetService<IMSGraphService>();

            if(graph.Client == null)
               await graph.SignInAsync();
        }
    }
}
