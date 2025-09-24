using BetterWidgets.Abstractions;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using BetterWidgets.Services;
using BetterWidgets.Views;
using System.Windows;
using BetterWidgets.Consts;
using BetterWidgets.Views.Dialogs;
using BetterWidgets.Properties;

namespace BetterWidgets.ViewModel
{
    public partial class WidgetDetailViewModel : ObservableObject
    {
        #region Services
        private readonly MainWindow Shell;
        private readonly IWidgetManager WidgetManager;
        private readonly Settings _settings;
        #endregion

        public WidgetDetailViewModel(IWidget widget, IWidgetManager widgetManager, Settings settings)
        {
            _settings = settings;

            Widget = widget;
            WidgetManager = widgetManager;
            Shell = Application.Current.MainWindow as MainWindow;

            var metadata = widgetManager?.GetWidgetById(widget.Id);

            PermissionsDataContext = new PermissionsViewModel(metadata);
        }

        #region Props

        [ObservableProperty]
        public bool isPrivacyFlyoutOpen = false;

        [ObservableProperty]
        public bool isWidgetPinned = false;

        public bool HasWidgetPermissions => Widget?.Permissions.Any() ?? false;

        private IWidget widget;
        public IWidget Widget
        {
            get => widget;
            set
            {
                widget = value;
                IsWidgetPinned = value?.IsPinnedDesktop ?? false;

                if(value != null && widget != value)
                   OnPropertyChanged(nameof(Widget));
            }
        }

        [ObservableProperty]
        public PermissionsViewModel permissionsDataContext;

        public bool AllowSharing => _settings.AllowSharing;

        #endregion

        #region Methods

        [RelayCommand]
        public void TogglePrivacyFlyout()
        {
            if(Widget == null || 
              !HasWidgetPermissions) return;

            var metadata = WidgetManager.GetWidgetById(Widget.Id);
               
            var dialog = new PermissionsDialog(metadata);
            dialog.ShowDialog();
        }

        [RelayCommand]
        public void PinWidget(IWidget widgetPreview)
        {
            if(widgetPreview == null) throw new ArgumentNullException(nameof(widgetPreview));

            var coreWidget = WidgetManager?.GetWidgetById(widgetPreview.Id);

            if(coreWidget == null) throw new InvalidOperationException(Errors.WidgetWithSpecifiedIdIsNotExists);

            if(!coreWidget.IsPinnedDesktop)
               WidgetManager?.PinToDesktop(coreWidget.Id);
            else 
               WidgetManager?.UnpinFromDesktop(coreWidget.Id);

            IsWidgetPinned = coreWidget.IsPinnedDesktop;
        }

        #endregion
    }
}
