using BetterWidgets.Abstractions;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.Views.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.Components
{
    public partial class WidgetViewItem : ObservableObject
    {
        private bool _active;

        #region Services
        private readonly ILogger _logger;
        private readonly Settings _settings;
        private readonly IWidgetManager _widgetManager;
        #endregion

        public WidgetViewItem(WidgetMetadata widget, IWidgetManager widgetManager)
        {
            _widgetManager = widgetManager;
            _logger = App.Services?.GetService<ILogger<WidgetViewItem>>();
            _settings = App.Services?.GetRequiredService<Settings>();

            Id = widget.Id;
            IsPinned = widget.IsPinnedDesktop;
            WidgetType = widget.Type;
            Title = widget.Title;
            Subtitle = widget.Subtitle;
            IconSource = new BitmapImage(widget.Icon);
            Permissions = new ObservableCollection<Permission>(widget.Permissions);
            DevMode = widget.DevMode;

            Icon = new ImageIcon()
            {
                Source = IconSource,
                Height = 15,
                Width = 15
            };

            _active = true;
        }

        #region Props

        public Type WidgetType { get; set; }

        [ObservableProperty]
        public Guid id;

        [ObservableProperty]
        public string title;

        [ObservableProperty]
        public string subtitle;

        [ObservableProperty]
        public bool devMode;

        [ObservableProperty]
        public ImageSource iconSource;

        [ObservableProperty]
        public IconElement icon;

        [ObservableProperty]
        public ObservableCollection<Permission> permissions;

        [ObservableProperty] public bool isPinned;

        [ObservableProperty] public bool isEnabled = true;

        public bool IsShowInTray
        {
            get => _settings.GetValue($"{WidgetType.Name}:{nameof(IsShowInTray)}", true);
            set
            {
                OnPropertyChanging(nameof(IsShowInTray));

                _settings.SetValue($"{WidgetType.Name}:{nameof(IsShowInTray)}", value);

                OnPropertyChanged(nameof(IsShowInTray));
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void Activate()
        {
            try
            {
                if(!_widgetManager.IsActivated(Id)) return;

                var widget = _widgetManager.GetActivatedWidget(Id);
                
                widget.ActivateWidget();
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Handlers

        async partial void OnIsPinnedChanged(bool oldValue, bool newValue)
        {
            if(!_active) return;
            if(newValue == oldValue) return;

            var trayWindow = App.Services?.GetRequiredService<TrayWindow>();

            if(newValue && !_widgetManager.IsActivated(Id))
            {
                IsEnabled = false;
                trayWindow.IsDeactivate = false;

                await Task.Run(async () =>
                {
                    await trayWindow.Dispatcher.InvokeAsync(() =>
                    {
                        _widgetManager.PinToDesktop(Id);
                    });
                });

                IsEnabled = true;
                trayWindow.IsDeactivate = true;
            }
            else if(_widgetManager.IsActivated(Id)) 
                _widgetManager.UnpinFromDesktop(Id);
        }

        #endregion
    }
}
