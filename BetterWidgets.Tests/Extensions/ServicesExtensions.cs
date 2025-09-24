using BetterWidgets.Services;
using BetterWidgets.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
using DataService = BetterWidgets.Tests.Services.DataService;

namespace BetterWidgets.Tests.Extensions
{
    internal static class ServicesExtensions
    {
        internal static IServiceCollection AddTestMSCalendarService(this IServiceCollection services)
            => services.AddScoped(typeof(ICalendarService<>), typeof(MSCalendarService<>));

        internal static IServiceCollection AddTestDataService(this IServiceCollection services)
            => services.AddScoped<IDataService, DataService>()
                       .AddScoped(typeof(DataService<>));

        internal static IServiceCollection AddTestPermissionManager(this IServiceCollection services)
            => services.AddScoped(typeof(IPermissionManager<>), typeof(TestPermissionManager<>));
    }
}
