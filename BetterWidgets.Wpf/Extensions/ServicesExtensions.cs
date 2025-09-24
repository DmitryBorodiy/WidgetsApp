using Wpf.Ui;
using BetterWidgets.Views;
using BetterWidgets.Properties;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using BetterWidgets.Services;
using BetterWidgets.Views.Settings;
using System.Net.Http;
using BetterWidgets.Model.Weather;
using Windows.UI.ViewManagement;
using BetterWidgets.Abstractions;
using System.Windows.Media;
using System.Diagnostics;
using BetterWidgets.Services.Hardware;
using BetterWidgets.Views.Windows;
using BetterWidgets.ViewModel;

namespace BetterWidgets.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
            => services?.AddSingleton<Configuration>();

        public static IServiceCollection AddDataService(this IServiceCollection services)
            => services.AddScoped<IDataService, DataService>()
                       .AddScoped(typeof(DataService<>));

        public static IServiceCollection AddTimeService(this IServiceCollection services)
            => services.AddScoped(typeof(ITimeService<>), typeof(TimeService<>));

        public static IServiceCollection AddSettings(this IServiceCollection services)
        {
            return services?.AddScoped<Settings>()
                            .AddScoped(typeof(Settings<>));
        }

        public static IServiceCollection AddUISettings(this IServiceCollection services)
            => services?.AddScoped<UISettings>();

        public static IServiceCollection AddMsalService(this IServiceCollection services)
            => services.AddScoped<IMsalService, MsalService>();

        public static IServiceCollection AddMSGraphService(this IServiceCollection services)
            => services.AddSingleton<IMSGraphService, MSGraphService>();

        public static IServiceCollection AddMSAccountInformationService(this IServiceCollection services)
            => services.AddScoped<IMSAccountInformation, MSAccountInformation>();

        public static IServiceCollection AddWindowsHello(this IServiceCollection services)
            => services.AddScoped<IWindowsHelloService, WindowsHelloService>();

        public static IServiceCollection AddThemeService(this IServiceCollection services)
            => services?.AddScoped<IThemeService, ThemeService>();

        public static IServiceCollection AddMainWindow(this IServiceCollection services)
            => services?.AddSingleton<MainWindowViewModel>()
                        .AddSingleton<MainWindow>();

        public static IServiceCollection AddNavigationFrame(this IServiceCollection services)
            => services?.AddTransient<Frame>();

        public static IServiceCollection AddSettingsManager(this IServiceCollection services)
            => services?.AddScoped<ISettingsManager, SettingsManager>();

        public static IServiceCollection AddSettingsView(this IServiceCollection services)
            => services?.AddScoped<SettingsView>();

        public static IServiceCollection AddPermissionManager(this IServiceCollection services)
            => services?.AddScoped<IPermissionManager, PermissionManager>()
                        .AddSingleton(typeof(IPermissionManager<>), typeof(PermissionManager<>));

        public static IServiceCollection AddClient(this IServiceCollection services)
            => services.AddScoped<HttpClient>();

        public static IServiceCollection AddGeolocationService(this IServiceCollection services)
            => services.AddSingleton(typeof(IGeolocationService<>), typeof(GeolocationService<>));

        public static IServiceCollection AddWidgetHttpClient(this IServiceCollection services)
            => services?.AddScoped(typeof(WidgetHttpClient<>));

        public static IServiceCollection AddWeatherService(this IServiceCollection services, Action<WeatherServiceOptions> options)
        {
            services.Configure(options);

            return services?.AddSingleton(typeof(IWeatherService<>), typeof(WeatherService<>));
        }

        public static IServiceCollection AddApplicationManager(this IServiceCollection services)
            => services.AddScoped<IApplicationManager, ApplicationManager>();

        public static IServiceCollection AddMSCalendarService(this IServiceCollection services)
            => services.AddScoped(typeof(ICalendarService<>), typeof(MSCalendarService<>));

        public static IServiceCollection AddTodoManager(this IServiceCollection services)
            => services.AddKeyedScoped(typeof(ITodoManager<>), nameof(MSTodoManager<IWidget>), typeof(MSTodoManager<>));


        public static IServiceCollection AddStickyNotes(this IServiceCollection services)
            => services.AddScoped(typeof(IStickyNotes<>), typeof(StickyNotes<>));

        public static IServiceCollection AddMediaPlayer(this IServiceCollection services)
            => services.AddScoped<MediaPlayer>()
                       .AddScoped<IMediaPlayerService, MediaPlayerService>();

        public static IServiceCollection AddCoreView(this IServiceCollection services)
            => services.AddScoped<CoreWidget>();

        public static IServiceCollection AddStoreService(this IServiceCollection services)
            => services.AddScoped<IStoreService, StoreService>();

        public static IServiceCollection AddSearch(this IServiceCollection services)
            => services.AddSingleton<ISearchService, SearchService>();

        public static IServiceCollection AddNavigation(this IServiceCollection services)
            => services.AddSingleton<INavigation, Navigation>();

        public static IServiceCollection AddTrayIcon(this IServiceCollection services)
            => services.AddSingleton<TrayIconService>();

        public static IServiceCollection AddShareService(this IServiceCollection services)
            => services.AddScoped<IShareService, ShareService>();

        public static IServiceCollection AddCpuWatcher(this IServiceCollection services)
            => services.AddScoped(s => new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                       .AddSingleton<ICpuWatcher, CpuWatcher>();

        public static IServiceCollection AddGpuWatcher(this IServiceCollection services)
            => services.AddScoped<GpuUpdateVisitor>()
                       .AddKeyedSingleton(typeof(IPerformanceWatcher<>), nameof(GpuPerformanceWatcher<IWidget>), typeof(GpuPerformanceWatcher<>));

        public static IServiceCollection AddSystemInformation(this IServiceCollection services)
            => services.AddTransient(typeof(ISystemInformation<>), typeof(SystemInformation<>));

        public static IServiceCollection AddTrayWindow(this IServiceCollection services)
            => services.AddSingleton<TrayWindow>();
    }
}
