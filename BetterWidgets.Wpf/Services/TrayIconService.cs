using Wpf.Ui.Tray;
using System.Windows;
using Microsoft.Extensions.Logging;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BetterWidgets.Views.Settings;
using System.Windows.Data;
using BetterWidgets.Views.Windows;

namespace BetterWidgets.Services
{
    [ObservableObject]
    public sealed partial class TrayIconService : NotifyIconService
    {
        #region Services
        private readonly ILogger _logger;
        private readonly INavigation _navigation;
        private readonly WidgetManager _widgets;
        private readonly CoreWidget _coreWidget;
        private readonly TrayWindow _trayWindow;
        #endregion

        public TrayIconService(ILogger<TrayIconService> logger, INavigation navigation, CoreWidget coreWidget, TrayWindow trayWindow)
        {
            _logger = logger;
            _navigation = navigation;
            _coreWidget = coreWidget;
            _trayWindow = trayWindow;
            _widgets = WidgetManager.Current;

            SetParentWindow(trayWindow);

            TooltipText = Resources.Resources.AppName;

            UIHideWidgetsItem = new MenuItem()
            {
                IsCheckable = true,
                Header = Resources.Resources.HideWidgets
            };
            UIHideWidgetsItem.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(IsWidgetsHidden))
            {
                Mode = BindingMode.TwoWay,
                Source = this
            });

            ContextMenu = new ContextMenu()
            {
                Items =
                {
                    new MenuItem()
                    {
                        Header = Resources.Resources.AddWidget,
                        Command = LaunchShellCommand
                    },
                    UIHideWidgetsItem,
                    new MenuItem()
                    {
                        Header = Resources.Resources.SettingsLabel,
                        Command = LaunchSettingsCommand
                    },
                    new Separator(),
                    new MenuItem()
                    {
                        Header = Resources.Resources.Close,
                        Command = ExitCommand
                    }
                }
            };
        }

        #region UI
        private MenuItem UIHideWidgetsItem;
        #endregion

        #region Props

        [ObservableProperty]
        public bool isWidgetsHidden;

        #endregion

        #region Commands

        [RelayCommand]
        private void LaunchShell()
        {
            try
            {
                _navigation.ActivateShell();
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        [RelayCommand]
        private void LaunchSettings()
        {
            var settings = new SettingsView();

            _navigation.ActivateShell();
            _navigation.Navigate(settings);
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Handlers

        partial void OnIsWidgetsHiddenChanged(bool value)
        {
            var activatedWidgets = _widgets.GetActivatedWidgets();
            var coreViews = _coreWidget.GetCreatedViews();

            if(activatedWidgets != null && activatedWidgets.Any())
            {
                foreach(var widget in activatedWidgets)
                {
                    if(!value) widget.ShowWidget();
                    else widget.HideWidget();
                }
            }

            if(coreViews != null && coreViews.Any())
            {
                foreach(var view in coreViews)
                {
                    if (!value) view.ShowWidget();
                    else view.HideWidget();
                }
            }
        }

        #endregion
    }
}
