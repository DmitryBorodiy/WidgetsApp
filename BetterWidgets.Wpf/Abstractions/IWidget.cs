using BetterWidgets.Enums;
using BetterWidgets.Events;
using BetterWidgets.Model;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace BetterWidgets.Abstractions
{
    public interface IWidget : ISearchable
    {
        Guid Id { get; }
        bool IsPreview { get; set; }
        bool IsPinnedDesktop { get; }
        bool IsRequireNetwork { get; }
        bool IsBlurEnabled { get; set; }
        bool IsNetworkAvailable { get; }
        bool IsSecondaryView { get; }
        bool CanShare { get; set; }
        bool DevMode { get; }
        string ProductId { get; }
        string Subtitle { get; set; }
        string WidgetGroupName { get; set; }
        double BackdropOpacity { get; set; }
        Size Size { get; set; }
        Point Position { get; set; }
        ImageSource Icon { get; set; }
        WidgetState State { get; }
        WidgetCornerMode CornerMode { get; set; }
        IEnumerable<Permission> Permissions { get; }

        ICommand AppearedCommand { get; set; }
        ICommand PinnedCommand { get; set; }
        ICommand UnpinCommand { get; set; }
        ICommand MouseEnterCommand { get; set; }
        ICommand MouseLeaveCommand { get; set; }
        ICommand ShareCommand { get; set; }
        ICommand StateChangedCommand { get; set; }

        object ShareCommandParameter { get; set; }

        void ShowWidget();
        void HideWidget();

        void ActivateWidget(bool activateWidgetBase = true);
        void PinOnDesktop();
        void UnpinDesktop();

        void SetExecutionState(WidgetState state);
        void SetWidgetLocationState(Point? point, bool cache = true);
        Point GetWidgetLocation();

        Size GetWidgetSize();
        void SetWidgetSize(Size? size, bool cache = true);

        void HideNotify();
        void ShowNotify(string message, string title = null, bool isClosable = false, InfoBarSeverity severity = InfoBarSeverity.Informational, bool hasDelay = false, TimeSpan delay = default);

        void ShowContentDialog(WidgetContentDialogParams parameters);
        void HideContentDialog(bool raiseCancel = false);
    }
}
