using Wpf.Ui;
using System.Windows;
using Wpf.Ui.Appearance;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using BetterWidgets.Extensions;
using BetterWidgets.Views;
using System.Globalization;
using BetterWidgets.Properties;
using BetterWidgets.Consts;
using BetterWidgets.Services;
using BetterWidgets.Helpers;
using System.Windows.Media;
using System.Windows.Shell;
using System.Diagnostics;
using BetterWidgets.Extensions.Logging;
using BetterWidgets.Abstractions;
using BetterWidgets.Views.Windows;

namespace BetterWidgets
{
    public partial class App : Application
    {
        public App()
        {
            InjectDependencies();
            InitLogging();

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var args = Environment.GetCommandLineArgs();
            var formattedArgs = string.Join(" ", args.Skip(1));

            if(!SingleInstanceManager.EnsureSingleInstance(nameof(BetterWidgets), OnCommandReceived))
            {
                SingleInstanceManager.SendCommandToExistingInstance(nameof(BetterWidgets), formattedArgs);
                Environment.Exit(0);

                return;
            }

            PreloadServices();
            ApplyTheme();
            ApplyCulture();
            SetupJumpList();
            SignInAsync();

            WidgetManager.Current.GetWidgets();

            MainWindow = Services?.GetService<MainWindow>();

            if(MainWindow != null)
            {
                MainWindow.Loaded += MainWindow_Loaded;
                MainWindow.Show();

                SetupTrayIcon();
            }
        }

        #region Service

        private ILogger Logger { get; set; }
        public static IServiceProvider Services { get; private set; }

        private static IThemeService Theme => Services?.GetService<IThemeService>();
        private static Settings Settings => Services?.GetService<Settings>();

        #endregion

        #region Props

        public static ApplicationTheme RequestedTheme
        {
            get => Theme?.GetTheme() ?? ApplicationTheme.Light;
            set => ReplaceTheme(value);
        }

        private bool IsCustomAccentColorEnabled
            => Settings?.GetValue<bool>(nameof(IsCustomAccentColorEnabled)) ?? false;

        private Color AccentColor
        {
            get
            {
                string hex = Settings?.GetValue(nameof(AccentColor), AccentColorHelper.AccentColor.ToHex());
                bool isEnabled = Settings?.GetValue(nameof(IsCustomAccentColorEnabled), false) ?? false;

                if(string.IsNullOrEmpty(hex) && !isEnabled) return AccentColorHelper.AccentColor;

                return (Color)ColorConverter.ConvertFromString(hex);
            }
        }

        #endregion

        #region Events
        public static event EventHandler<ApplicationTheme> ThemeChanged;
        #endregion

        #region Utils

        private void ApplyTheme()
        {
            var settings = Services?.GetService<Settings>();

            if(settings.ContainsKey(nameof(settings.IsDarkMode)))
               RequestedTheme = settings.IsDarkMode ? 
                                ApplicationTheme.Dark : ApplicationTheme.Light;
            else
               RequestedTheme = Theme?.GetSystemTheme() ?? ApplicationTheme.Light;

            bool isDarkColor = RequestedTheme == ApplicationTheme.Dark;

            if(IsCustomAccentColorEnabled) 
               AccentColorHelper.SetAccentColor(isDarkColor, AccentColor);
            else
               AccentColorHelper.SetSystemAccentColor(isDarkColor);
        }

        private void ApplyCulture()
        {
            var settings = Services?.GetService<Settings>();

            CultureInfo.CurrentCulture = settings.AppLanguage;
            CultureInfo.CurrentUICulture = settings.AppLanguage;
        }

        private async void SignInAsync()
        {
            if(!NetworkHelper.IsConnected) return;

            var graphService = Services?.GetService<IMSGraphService>();

            if(graphService.IsSignedIn && graphService.Client == null)
               await graphService.SignInAsync();
        }

        private void InjectDependencies()
        {
            var builder = new ServiceCollection();

            builder.AddLogging(b => {
                b.AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Debug);
#if DEBUG
                b.AddSimpleConsole()
                .AllocConsole();
#endif
            });
            builder.AddConfiguration();
            builder.AddDataService();
            builder.AddTimeService();
            builder.AddSettings();
            builder.AddUISettings();
            builder.AddClient();
            builder.AddSearch();
            builder.AddWidgetHttpClient();
            builder.AddPermissionManager();
            builder.AddGeolocationService();
            builder.AddWindowsHello();
            builder.AddMsalService();
            builder.AddMSGraphService();
            builder.AddMSAccountInformationService();
            builder.AddMainWindow();
            builder.AddCoreView();
            builder.AddNavigationFrame();
            builder.AddThemeService();
            builder.AddSettingsManager();
            builder.AddSettingsView();
            builder.AddApplicationManager();
            builder.AddMSCalendarService();
            builder.AddTodoManager();
            builder.AddStickyNotes();
            builder.AddMediaPlayer();
            builder.AddStoreService();
            builder.AddNavigation();
            builder.AddTrayIcon();
            builder.AddShareService();
            builder.AddCpuWatcher();
            builder.AddSystemInformation();
            builder.AddGpuWatcher();
            builder.AddTrayWindow();
            builder.AddWeatherService(o =>
            {
                o.ApiKey = ApiKeys.OpenWeatherMap;
            });

            Services = builder.BuildServiceProvider();
        }

        private void PreloadServices()
        {
            Services?.GetService<ICpuWatcher>();
        }

        private void InitLogging()
        {
            Logger = Services?.GetService<ILogger<App>>();
        }

        private void SetupJumpList()
        {
            try
            {
                var jumpList = new JumpList()
                {
                    JumpItems =
                    {
                        new JumpTask
                        {
                            Title = BetterWidgets.Resources.Resources.AddWidget,
                            ApplicationPath = ShellCommands.widgets,
                            Arguments = ShellCommands.addwidget,
                            IconResourcePath = Process.GetCurrentProcess().MainModule.FileName,
                            IconResourceIndex = 0
                        },
                        new JumpTask
                        {
                            Title = BetterWidgets.Resources.Resources.HideWidgets,
                            ApplicationPath = ShellCommands.widgets,
                            Arguments = ShellCommands.hidewidget,
                            IconResourcePath = Process.GetCurrentProcess().MainModule.FileName,
                            IconResourceIndex = 0
                        },
                        new JumpTask
                        {
                            Title = BetterWidgets.Resources.Resources.SettingsLabel,
                            ApplicationPath = ShellCommands.widgets,
                            Arguments = ShellCommands.settings,
                            IconResourcePath = Process.GetCurrentProcess().MainModule.FileName,
                            IconResourceIndex = 0
                        }
                    }
                };

                jumpList.ShowFrequentCategory = false;
                jumpList.ShowRecentCategory = false;
                JumpList.SetJumpList(Current, jumpList);
            }
            catch(Exception ex)
            {
                Logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        private void SetupTrayIcon()
        {
            try
            {
                var trayWindow = Services?.GetRequiredService<TrayWindow>();
                var trayIcon = Services?.GetRequiredService<TrayIconService>();

                if(Settings.IsTrayIconEnabled)
                {
                    MainWindow = trayWindow;
                    MainWindow.Show();

                    if(!trayIcon.Register())
                       throw new InvalidOperationException(Errors.CannotRegisterTrayIcon);

                    MainWindow.Hide();
                }
            }
            catch(Exception ex)
            {
                Logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        private static void ReplaceTheme(ApplicationTheme theme)
        {
            Theme?.SetTheme(theme);
            ThemeChanged?.Invoke(null, theme);

            AccentColorHelper.SetSystemAccentColor(theme == ApplicationTheme.Dark);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
            => Logger?.LogError(exception: e.Exception, e.Exception?.Message);

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
            => Logger?.LogError(exception: e.Exception, e.Exception?.Message);

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(10);

            var manager = WidgetManager.Current;

            if(manager.Widgets.Values.Any(w => w.IsPinnedDesktop))
               Current.MainWindow.WindowState = WindowState.Minimized;

            if(!Settings.ShowMainWindowOnStartup)
               MainWindow.Hide();
        }

#endregion

        #region Handlers

        private void OnCommandReceived(string args)
        {
            var trayIcon = Services.GetService<TrayIconService>();

            switch(args)
            {
                case ShellCommands.addwidget:
                    trayIcon.LaunchShellCommand.Execute(null);
                    break;
                case ShellCommands.hidewidget:
                    trayIcon.IsWidgetsHidden = !trayIcon.IsWidgetsHidden;
                    break;
                case ShellCommands.settings:
                    trayIcon.LaunchSettingsCommand.Execute(null);
                    break;
            }
        }

        #endregion
    }
}
